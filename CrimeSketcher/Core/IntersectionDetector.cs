// Core/IntersectionDetector.cs
using CrimeSketcher.Objects;
using CrimeSketcher.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CrimeSketcher.Core
{
    /// <summary>
    /// Detecta automaticamente cruzamentos entre ruas
    /// </summary>
    public static class IntersectionDetector
    {
        private const float TOLERANCIA_INTERSECAO = 2f;
        private const float DISTANCIA_MINIMA_MERGE = 10f;

        /// <summary>
        /// Verifica se uma nova rua intercepta ruas existentes e retorna possíveis cruzamentos
        /// </summary>
        public static List<(StreetObject rua1, StreetObject rua2, PointF ponto, IntersectionType tipo)> 
            DetectarCruzamentos(StreetObject novaRua, IEnumerable<BaseSketchObject> objetosExistentes)
        {
            var cruzamentos = new List<(StreetObject, StreetObject, PointF, IntersectionType)>();

            foreach (var obj in objetosExistentes)
            {
                if (obj is not StreetObject ruaExistente)
                    continue;

                var interceptos = DetectarIntercepcao(novaRua, ruaExistente);
                foreach (var (ponto, tipo) in interceptos)
                {
                    cruzamentos.Add((novaRua, ruaExistente, ponto, tipo));
                }
            }

            return cruzamentos;
        }

        /// <summary>
        /// Detecta onde duas ruas se interceptam (se é um T ou uma Cruz).
        /// 
        /// Estratégia de detecção:
        ///   1. Cruzamento completo (Cruz ou T): ambos os eixos centrais se cruzam
        ///      dentro dos segmentos (t,s ∈ [0,1]). Se ambos estão próximos ao
        ///      meio → Cruz (4 vias); caso contrário → T (3 vias).
        ///   2. Primeiro toque (T): o eixo central de uma rua não alcança o eixo da
        ///      outra, mas sua extremidade já entrou na faixa de largura da outra rua
        ///      (distância ≤ Largura/2). O ponto de cruzamento é projetado na
        ///      interseção das retas (extensão infinita dos eixos).
        /// </summary>
        private static List<(PointF ponto, IntersectionType tipo)> DetectarIntercepcao(
            StreetObject rua1, 
            StreetObject rua2)
        {
            var resultados = new List<(PointF, IntersectionType)>();

            // Encontrar a interseção dos eixos centrais (tratados como retas infinitas)
            if (!EncontrarIntersecaoRetas(rua1, rua2, out var pontoIntersecao, out float t, out float s))
                return resultados;

            const float MARGEM = 0.05f;
            bool tNoSegmento = t >= -MARGEM && t <= 1f + MARGEM;
            bool sNoSegmento = s >= -MARGEM && s <= 1f + MARGEM;

            if (tNoSegmento && sNoSegmento)
            {
                // Caso 1: eixos centrais se cruzam dentro de ambos os segmentos
                if (!EstaIntersecaoDentroDeAmbas(pontoIntersecao, rua1, rua2))
                    return resultados;

                var tipo = DeterminarTipoCruzamento(pontoIntersecao, rua1, rua2);
                resultados.Add((pontoIntersecao, tipo));
            }
            else if (sNoSegmento && !tNoSegmento)
            {
                // Caso 2: eixo de rua1 não alcança o eixo de rua2.
                // Verificar se a extremidade de rua1 toca a faixa de rua2 → T
                if (ExtremoTocaRua(rua1, rua2) &&
                    !EstaProximoExtremidade(pontoIntersecao, rua2))
                {
                    resultados.Add((pontoIntersecao, IntersectionType.T));
                }
            }
            else if (tNoSegmento && !sNoSegmento)
            {
                // Caso 3: eixo de rua2 não alcança o eixo de rua1.
                if (ExtremoTocaRua(rua2, rua1) &&
                    !EstaProximoExtremidade(pontoIntersecao, rua1))
                {
                    resultados.Add((pontoIntersecao, IntersectionType.T));
                }
            }

            return resultados;
        }

        /// <summary>
        /// Encontra o ponto de interseção das retas que contêm os eixos centrais
        /// de duas ruas (tratadas como retas infinitas, sem restrição de segmento).
        /// Retorna os parâmetros t e s das equações paramétricas:
        ///   rua1: P₁ + t(P₂ - P₁)
        ///   rua2: P₃ + s(P₄ - P₃)
        /// t ∈ [0,1] significa que o ponto está no segmento de rua1; analogamente para s.
        /// </summary>
        private static bool EncontrarIntersecaoRetas(
            StreetObject rua1, StreetObject rua2,
            out PointF ponto, out float t, out float s)
        {
            ponto = PointF.Empty;
            t = s = 0f;

            var p1 = rua1.PontoInicial;
            var p2 = rua1.PontoFinal;
            var p3 = rua2.PontoInicial;
            var p4 = rua2.PontoFinal;

            float dx1 = p2.X - p1.X;
            float dy1 = p2.Y - p1.Y;
            float dx2 = p4.X - p3.X;
            float dy2 = p4.Y - p3.Y;

            float denom = dx1 * dy2 - dy1 * dx2;

            // Se denom é muito próximo de zero, as retas são paralelas
            if (Math.Abs(denom) < TOLERANCIA_INTERSECAO)
                return false;

            float dx3 = p3.X - p1.X;
            float dy3 = p3.Y - p1.Y;

            t = (dx3 * dy2 - dy3 * dx2) / denom;
            s = (dx3 * dy1 - dy3 * dx1) / denom;

            ponto = new PointF(
                p1.X + t * dx1,
                p1.Y + t * dy1);
            return true;
        }

        /// <summary>
        /// Verifica se algum extremo (PontoInicial ou PontoFinal) de ruaCurta
        /// está dentro da faixa de largura de ruaLonga (distância perpendicular
        /// ao eixo central ≤ Largura/2). Usado para detectar o "primeiro toque"
        /// em cruzamentos T, antes de o eixo central alcançar o eixo da outra rua.
        /// </summary>
        private static bool ExtremoTocaRua(StreetObject ruaCurta, StreetObject ruaLonga)
        {
            float distInicio = DistanciaAoSegmento(
                ruaCurta.PontoInicial, ruaLonga.PontoInicial, ruaLonga.PontoFinal);
            float distFim = DistanciaAoSegmento(
                ruaCurta.PontoFinal, ruaLonga.PontoInicial, ruaLonga.PontoFinal);

            float limiar = ruaLonga.Largura / 2f + TOLERANCIA_INTERSECAO;
            return distInicio <= limiar || distFim <= limiar;
        }

        /// <summary>
        /// Verifica se o ponto de interseção está realmente dentro da área de ambas as ruas
        /// </summary>
        private static bool EstaIntersecaoDentroDeAmbas(PointF ponto, StreetObject rua1, StreetObject rua2)
        {
            // Verificar se o ponto está dentro da largura de rua1
            float distanciaA1 = DistanciaAoSegmento(ponto, rua1.PontoInicial, rua1.PontoFinal);
            if (distanciaA1 > rua1.Largura / 2f + TOLERANCIA_INTERSECAO)
                return false;

            // Verificar se o ponto está dentro da largura de rua2
            float distanciaA2 = DistanciaAoSegmento(ponto, rua2.PontoInicial, rua2.PontoFinal);
            if (distanciaA2 > rua2.Largura / 2f + TOLERANCIA_INTERSECAO)
                return false;

            // Verificar se não está muito perto das extremidades
            if (EstaProximoExtremidade(ponto, rua1) && EstaProximoExtremidade(ponto, rua2))
                return false;

            return true;
        }

        /// <summary>
        /// Determina se o cruzamento é um T ou uma Cruz
        /// </summary>
        private static IntersectionType DeterminarTipoCruzamento(PointF ponto, StreetObject rua1, StreetObject rua2)
        {
            // Calcular a posição relativa do ponto no eixo de cada rua
            float posRelativa1 = ObterPosicaoRelativaNoSegmento(ponto, rua1.PontoInicial, rua1.PontoFinal);
            float posRelativa2 = ObterPosicaoRelativaNoSegmento(ponto, rua2.PontoInicial, rua2.PontoFinal);

            // Se ambas as posições estão próximas ao meio (0.4 a 0.6), é uma Cruz
            bool no_meio_1 = posRelativa1 > 0.4f && posRelativa1 < 0.6f;
            bool no_meio_2 = posRelativa2 > 0.4f && posRelativa2 < 0.6f;

            if (no_meio_1 && no_meio_2)
                return IntersectionType.Cruz;

            // Caso contrário, é um T
            return IntersectionType.T;
        }

        /// <summary>
        /// Calcula a distância perpendicular de um ponto a um segmento de reta
        /// </summary>
        private static float DistanciaAoSegmento(PointF ponto, PointF p1, PointF p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float comp2 = dx * dx + dy * dy;

            if (comp2 == 0)
                return GeometryHelper.Distancia(ponto, p1);

            float t = Math.Max(0, Math.Min(1, 
                ((ponto.X - p1.X) * dx + (ponto.Y - p1.Y) * dy) / comp2));

            var projecao = new PointF(
                p1.X + t * dx,
                p1.Y + t * dy);

            return GeometryHelper.Distancia(ponto, projecao);
        }

        /// <summary>
        /// Obtém a posição relativa do ponto no segmento (0 = início, 1 = fim)
        /// </summary>
        private static float ObterPosicaoRelativaNoSegmento(PointF ponto, PointF p1, PointF p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float comp2 = dx * dx + dy * dy;

            if (comp2 == 0)
                return 0f;

            return ((ponto.X - p1.X) * dx + (ponto.Y - p1.Y) * dy) / comp2;
        }

        /// <summary>
        /// Verifica se um ponto está próximo da extremidade de uma rua
        /// </summary>
        private static bool EstaProximoExtremidade(PointF ponto, StreetObject rua)
        {
            float distInicio = GeometryHelper.Distancia(ponto, rua.PontoInicial);
            float distFim = GeometryHelper.Distancia(ponto, rua.PontoFinal);
            float limiar = Math.Max(20f, rua.Largura * 0.75f);

            return distInicio < limiar || distFim < limiar;
        }

        /// <summary>
        /// Verifica se o ponto está muito perto de uma das extremidades
        /// </summary>
        public static bool EstaProximoDeExtremidade(PointF ponto, StreetObject rua, float tolerancia)
        {
            float distInicio = GeometryHelper.Distancia(ponto, rua.PontoInicial);
            float distFim = GeometryHelper.Distancia(ponto, rua.PontoFinal);

            return distInicio < tolerancia || distFim < tolerancia;
        }

        /// <summary>
        /// Encontra a rua mais próxima que este ponto deveria conectar
        /// </summary>
        public static StreetObject FindNearestStreet(PointF ponto, IEnumerable<BaseSketchObject> objetos, 
            out float distancia, StreetObject excluir = null)
        {
            distancia = float.MaxValue;
            StreetObject ruaMaisProxima = null;

            foreach (var obj in objetos)
            {
                if (obj is not StreetObject rua || rua == excluir)
                    continue;

                float dist = DistanciaAoSegmento(ponto, rua.PontoInicial, rua.PontoFinal);
                if (dist < distancia)
                {
                    distancia = dist;
                    ruaMaisProxima = rua;
                }
            }

            return ruaMaisProxima;
        }
    }

    /// <summary>
    /// Tipo de cruzamento detectado
    /// </summary>
    public enum IntersectionType
    {
        T,      // Cruzamento em T (3 vias)
        Cruz    // Cruzamento em Cruz (4 vias)
    }
}
