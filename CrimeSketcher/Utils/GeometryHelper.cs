// Utils/GeometryHelper.cs
using System;
using System.Drawing;

namespace CrimeSketcher.Utils
{
    public static class GeometryHelper
    {
        private const float EPSILON = 0.0001f;
        private const float EPSILON_DISTANCIA = 0.001f;
        private const float MIN_PASSO_ANGULAR = 0.1f;
        private const float DELTA_RAIO_MINIMO = 0.01f;
        private const float GRAUS_PARA_RAD = (float)(Math.PI / 180.0);
        private const float RAD_PARA_GRAUS = (float)(180.0 / Math.PI);
        private const float LIMIAR_FECHAMENTO_CIRCULO_GRAUS = 340f;
        private const float VARREDURA_MAXIMA_CIRCULO_GRAUS = 359.5f;

        /// <summary>
        /// Calcula a distância de um ponto a um segmento de reta
        /// </summary>
        public static float DistanciaPontoSegmento(PointF ponto,
            PointF segA, PointF segB)
        {
            float dx = segB.X - segA.X;
            float dy = segB.Y - segA.Y;
            float comprimento2 = dx * dx + dy * dy;

            if (comprimento2 <= EPSILON)
                return Distancia(ponto, segA);

            float projecao = ((ponto.X - segA.X) * dx + (ponto.Y - segA.Y) * dy) / comprimento2;
            float t = Clamp01(projecao);

            var proj = new PointF(segA.X + t * dx, segA.Y + t * dy);
            return Distancia(ponto, proj);
        }

        public static float Distancia(PointF a, PointF b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static float AnguloGraus(PointF origem, PointF destino)
        {
            return (float)(Math.Atan2(
                destino.Y - origem.Y,
                destino.X - origem.X) * RAD_PARA_GRAUS);
        }

        public static PointF PontoMedio(PointF a, PointF b)
        {
            return new PointF((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        }

        public static PointF RotacionarPonto(PointF ponto,
            PointF centro, float anguloGraus)
        {
            float rad = anguloGraus * GRAUS_PARA_RAD;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            float dx = ponto.X - centro.X;
            float dy = ponto.Y - centro.Y;

            return new PointF(
                centro.X + dx * cos - dy * sin,
                centro.Y + dx * sin + dy * cos);
        }

        public static bool TryGetCircunferenciaPorTresPontos(
            PointF a,
            PointF b,
            PointF c,
            out PointF centro,
            out float raio)
        {
            float d = 2f * (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));
            if (Math.Abs(d) < EPSILON)
            {
                centro = PointF.Empty;
                raio = 0f;
                return false;
            }

            float a2 = a.X * a.X + a.Y * a.Y;
            float b2 = b.X * b.X + b.Y * b.Y;
            float c2 = c.X * c.X + c.Y * c.Y;

            float ux = (a2 * (b.Y - c.Y) + b2 * (c.Y - a.Y) + c2 * (a.Y - b.Y)) / d;
            float uy = (a2 * (c.X - b.X) + b2 * (a.X - c.X) + c2 * (b.X - a.X)) / d;

            centro = new PointF(ux, uy);
            raio = Distancia(centro, a);
            return raio > EPSILON;
        }

        public static bool TryGetArcoCircular(
            PointF inicio,
            PointF passagem,
            PointF fim,
            out PointF centro,
            out float raio,
            out float anguloInicialGraus,
            out float varreduraGraus)
        {
            if (!TryGetCircunferenciaPorTresPontos(inicio, passagem, fim, out centro, out raio))
            {
                anguloInicialGraus = 0f;
                varreduraGraus = 0f;
                return false;
            }

            float angInicio = NormalizarAngulo360(AnguloGraus(centro, inicio));
            float angPassagem = NormalizarAngulo360(AnguloGraus(centro, passagem));
            float angFim = NormalizarAngulo360(AnguloGraus(centro, fim));

            float sweepCcw = DeltaAnguloCcw(angInicio, angFim);
            float sweepCcwPassagem = DeltaAnguloCcw(angInicio, angPassagem);

            if (sweepCcw > EPSILON && sweepCcwPassagem > EPSILON && sweepCcwPassagem < sweepCcw)
            {
                anguloInicialGraus = angInicio;
                varreduraGraus = sweepCcw;
                return true;
            }

            float sweepCw = DeltaAnguloCw(angInicio, angFim);
            float sweepCwPassagem = DeltaAnguloCw(angInicio, angPassagem);
            if (sweepCw < -EPSILON && sweepCwPassagem < -EPSILON && sweepCwPassagem > sweepCw)
            {
                anguloInicialGraus = angInicio;
                varreduraGraus = sweepCw;
                return true;
            }

            anguloInicialGraus = angInicio;
            varreduraGraus = Math.Abs(sweepCcw) <= Math.Abs(sweepCw) ? sweepCcw : sweepCw;
            return true;
        }

        public static PointF ObterPontoArcoCircular(
            PointF centro,
            float raio,
            float anguloInicialGraus,
            float varreduraGraus,
            float t)
        {
            float angulo = anguloInicialGraus + varreduraGraus * t;
            float rad = angulo * GRAUS_PARA_RAD;
            return new PointF(
                centro.X + raio * (float)Math.Cos(rad),
                centro.Y + raio * (float)Math.Sin(rad));
        }

        public static PointF ObterTangenteArcoCircular(float anguloGraus, bool sentidoAntiHorario)
        {
            float rad = anguloGraus * GRAUS_PARA_RAD;
            float tx = -(float)Math.Sin(rad);
            float ty = (float)Math.Cos(rad);

            if (!sentidoAntiHorario)
            {
                tx = -tx;
                ty = -ty;
            }

            float len = (float)Math.Sqrt(tx * tx + ty * ty);
            if (len <= EPSILON)
                return new PointF(1f, 0f);

            return new PointF(tx / len, ty / len);
        }

        public static bool TryGetPontoCurvaArcoPorRaio(
            PointF inicio,
            PointF fim,
            float raioDesejado,
            PointF referenciaLado,
            out PointF pontoCurva)
        {
            return TryGetPontoCurvaArcoPorRaio(inicio, fim, raioDesejado, referenciaLado, null, out pontoCurva);
        }

        public static bool TryGetPontoCurvaArcoPorRaio(
            PointF inicio,
            PointF fim,
            float raioDesejado,
            PointF referenciaLado,
            float? sweepPreferidoGraus,
            out PointF pontoCurva)
        {
            pontoCurva = PontoMedio(inicio, fim);

            float dx = fim.X - inicio.X;
            float dy = fim.Y - inicio.Y;
            float comprimento = (float)Math.Sqrt(dx * dx + dy * dy);
            if (comprimento <= EPSILON)
                return false;

            float meiaCorda = comprimento / 2f;
            float raio = Math.Max(Math.Abs(raioDesejado), meiaCorda + DELTA_RAIO_MINIMO);
            float h = (float)Math.Sqrt(Math.Max(0f, raio * raio - meiaCorda * meiaCorda));

            var meio = PontoMedio(inicio, fim);
            var perp = new PointF(-dy / comprimento, dx / comprimento);
            float lado = Math.Sign((referenciaLado.X - meio.X) * perp.X + (referenciaLado.Y - meio.Y) * perp.Y);
            if (lado == 0f)
                lado = 1f;

            if (!sweepPreferidoGraus.HasValue)
            {
                pontoCurva = new PointF(
                    meio.X + perp.X * lado * (raio - h),
                    meio.Y + perp.Y * lado * (raio - h));
                return true;
            }

            var centro = new PointF(
                meio.X - perp.X * lado * h,
                meio.Y - perp.Y * lado * h);

            float angInicio = NormalizarAngulo360(AnguloGraus(centro, inicio));
            float angFim = NormalizarAngulo360(AnguloGraus(centro, fim));
            float sweep = sweepPreferidoGraus.Value >= 0f
                ? DeltaAnguloCcw(angInicio, angFim)
                : DeltaAnguloCw(angInicio, angFim);

            pontoCurva = ObterPontoArcoCircular(centro, raio, angInicio, sweep, 0.5f);
            return true;
        }

        public static bool LimitarFechamentoArcoCircular(ref PointF inicio, ref PointF passagem, ref PointF fim)
        {
            if (!TryGetArcoCircular(inicio, passagem, fim, out var centro, out var raio, out var anguloInicial, out var varredura))
                return false;

            if (Math.Abs(varredura) < LIMIAR_FECHAMENTO_CIRCULO_GRAUS)
                return false;

            float varreduraLimitada = Math.Sign(varredura) * VARREDURA_MAXIMA_CIRCULO_GRAUS;
            fim = ObterPontoArcoCircular(centro, raio, anguloInicial, varreduraLimitada, 1f);
            return true;
        }

        public static PointF SnapAngulo(PointF origem, PointF ponto, float incrementoGraus)
        {
            float dx = ponto.X - origem.X;
            float dy = ponto.Y - origem.Y;
            float distancia = (float)Math.Sqrt(dx * dx + dy * dy);
            if (distancia < EPSILON_DISTANCIA) return ponto;

            float passo = Math.Max(MIN_PASSO_ANGULAR, incrementoGraus);
            float angulo = (float)(Math.Atan2(dy, dx) * RAD_PARA_GRAUS);
            float anguloSnapped = (float)(Math.Round(angulo / passo) * passo);
            float rad = anguloSnapped * GRAUS_PARA_RAD;

            return new PointF(
                origem.X + distancia * (float)Math.Cos(rad),
                origem.Y + distancia * (float)Math.Sin(rad));
        }

        public static PointF SnapAngulo15(PointF origem, PointF ponto)
        {
            return SnapAngulo(origem, ponto, 15f);
        }

        public static float NormalizarAngulo360(float angulo)
        {
            float normalizado = angulo % 360f;
            if (normalizado < 0f)
                normalizado += 360f;

            if (Math.Abs(normalizado) < EPSILON || Math.Abs(normalizado - 360f) < EPSILON)
                return 0f;

            return normalizado;
        }

        private static float Clamp01(float valor)
            => Math.Max(0f, Math.Min(1f, valor));

        private static float DeltaAnguloCcw(float anguloInicial, float anguloFinal)
        {
            return NormalizarAngulo360(anguloFinal - anguloInicial);
        }

        private static float DeltaAnguloCw(float anguloInicial, float anguloFinal)
        {
            return -NormalizarAngulo360(anguloInicial - anguloFinal);
        }
    }
}