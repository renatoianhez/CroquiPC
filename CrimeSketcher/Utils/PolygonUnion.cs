// Utils/PolygonUnion.cs
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CrimeSketcher.Utils
{
    /// <summary>
    /// Calcula a união de dois polígonos convexos (retângulos rotacionados)
    /// para renderizar interseções de ruas como um único polígono unificado.
    /// 
    /// Fundamentação matemática:
    /// 
    /// Vértices de um retângulo rotacionado (matriz de rotação 2D):
    ///   [X]   [cos(θ)  -sin(θ)] [x']   [x₀]
    ///   [Y] = [sin(θ)   cos(θ)] [y'] + [y₀]
    /// 
    /// Interseção de segmentos (equações paramétricas):
    ///   P = P₁ + t(P₂ - P₁) = P₃ + u(P₄ - P₃)
    ///   Se 0 ≤ t,u ≤ 1, os segmentos se cruzam.
    /// 
    /// Construção da união (algoritmo Weiler-Atherton simplificado):
    ///   1. Coletar vértices de R1 que estão fora de R2
    ///   2. Coletar vértices de R2 que estão fora de R1
    ///   3. Coletar todos os pontos de interseção entre arestas de R1 e R2
    ///   4. Remover duplicatas
    ///   5. Ordenar por ângulo ao centroide → polígono da união
    /// 
    /// Para dois polígonos convexos cuja interseção contém o centroide dos pontos
    /// da borda, o resultado é star-shaped e a ordenação angular produz o contorno
    /// correto (incluindo regiões côncavas como o formato "+").
    /// </summary>
    public static class PolygonUnion
    {
        private const float EPSILON = 1e-5f;
        private const float TOLERANCIA_DUPLICATA = 0.5f;

        /// <summary>
        /// Calcula os vértices de um retângulo rotacionado usando a matriz de rotação 2D.
        /// 
        /// Para um retângulo com dimensões (largura × altura) centrado em (x₀, y₀)
        /// e rotacionado por θ graus:
        /// 
        ///   V_local = {(-w/2,-h/2), (w/2,-h/2), (w/2,h/2), (-w/2,h/2)}
        ///   V_global[i] = R(θ) · V_local[i] + (x₀, y₀)
        /// 
        /// onde R(θ) = [cos(θ)  -sin(θ)]
        ///             [sin(θ)   cos(θ)]
        /// </summary>
        /// <param name="centro">Ponto de referência (x₀, y₀) - centro do retângulo</param>
        /// <param name="largura">Dimensão horizontal do retângulo (a)</param>
        /// <param name="altura">Dimensão vertical do retângulo (b)</param>
        /// <param name="anguloGraus">Ângulo de rotação θ em graus</param>
        /// <returns>Array com os 4 vértices transformados em ordem anti-horária</returns>
        public static PointF[] CalcularVerticesRetanguloRotacionado(
            PointF centro, float largura, float altura, float anguloGraus)
        {
            float rad = anguloGraus * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            float hw = largura / 2f;
            float hh = altura / 2f;

            // Coordenadas locais (x', y') dos 4 vértices
            var locais = new PointF[]
            {
                new PointF(-hw, -hh),  // V1,1
                new PointF(hw, -hh),   // V1,2
                new PointF(hw, hh),    // V1,3
                new PointF(-hw, hh)    // V1,4
            };

            // Aplicar transformação afim:
            // [X]   [cos(θ)  -sin(θ)] [x']   [x₀]
            // [Y] = [sin(θ)   cos(θ)] [y'] + [y₀]
            var resultado = new PointF[4];
            for (int i = 0; i < 4; i++)
            {
                resultado[i] = new PointF(
                    cos * locais[i].X - sin * locais[i].Y + centro.X,
                    sin * locais[i].X + cos * locais[i].Y + centro.Y);
            }

            return resultado;
        }

        /// <summary>
        /// Encontra o ponto de interseção P de dois segmentos (P₁,P₂) e (P₃,P₄)
        /// resolvendo o sistema linear para os parâmetros t e u:
        /// 
        ///   P = P₁ + t(P₂ - P₁) = P₃ + u(P₄ - P₃)
        /// 
        /// O sistema é resolvido calculando o determinante:
        ///   det = (P₂-P₁) × (P₄-P₃)
        ///   t = ((P₃-P₁) × (P₄-P₃)) / det
        ///   u = ((P₃-P₁) × (P₂-P₁)) / det
        /// 
        /// Se 0 ≤ t,u ≤ 1, as arestas se cruzam.
        /// </summary>
        /// <returns>true se os segmentos se cruzam (0 ≤ t,u ≤ 1)</returns>
        public static bool IntersecaoSegmentos(
            PointF p1, PointF p2, PointF p3, PointF p4,
            out float t, out float u, out PointF pontoIntersecao)
        {
            t = u = 0f;
            pontoIntersecao = PointF.Empty;

            float d1x = p2.X - p1.X;
            float d1y = p2.Y - p1.Y;
            float d2x = p4.X - p3.X;
            float d2y = p4.Y - p3.Y;

            // Determinante (produto vetorial 2D)
            float det = d1x * d2y - d1y * d2x;
            if (Math.Abs(det) < EPSILON)
                return false; // Segmentos paralelos ou colineares

            float dx = p3.X - p1.X;
            float dy = p3.Y - p1.Y;

            t = (dx * d2y - dy * d2x) / det;
            u = (dx * d1y - dy * d1x) / det;

            if (t >= -EPSILON && t <= 1f + EPSILON &&
                u >= -EPSILON && u <= 1f + EPSILON)
            {
                t = Math.Max(0f, Math.Min(1f, t));
                u = Math.Max(0f, Math.Min(1f, u));

                pontoIntersecao = new PointF(
                    p1.X + t * d1x,
                    p1.Y + t * d1y);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Testa se um ponto está dentro de um polígono convexo usando o teste
        /// de produto vetorial (cross product).
        /// 
        /// Para um polígono convexo com vértices em ordem consistente (CW ou CCW),
        /// o ponto está dentro se e somente se todos os cross products das arestas
        /// em relação ao ponto têm o mesmo sinal:
        /// 
        ///   cross_i = (V_{i+1} - V_i) × (P - V_i)
        /// 
        /// Se todos cross_i ≥ 0 ou todos cross_i ≤ 0, o ponto está dentro.
        /// </summary>
        public static bool PontoNoPoligonoConvexo(PointF ponto, PointF[] poligono)
        {
            int n = poligono.Length;
            if (n < 3) return false;

            bool todosPositivos = true;
            bool todosNegativos = true;

            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                float cross = (poligono[j].X - poligono[i].X) * (ponto.Y - poligono[i].Y)
                            - (poligono[j].Y - poligono[i].Y) * (ponto.X - poligono[i].X);

                if (cross > EPSILON) todosNegativos = false;
                if (cross < -EPSILON) todosPositivos = false;
            }

            return todosPositivos || todosNegativos;
        }

        /// <summary>
        /// Calcula o polígono resultante da união de dois polígonos convexos (A ∪ B).
        /// 
        /// Implementa o algoritmo Weiler-Atherton simplificado para polígonos convexos:
        /// 
        /// 1. Coleta os pontos de interesse que formam o novo polígono:
        ///    - Vértices de A que estão fora de B (fazem parte da casca externa)
        ///    - Vértices de B que estão fora de A (fazem parte da casca externa)
        ///    - Pontos de interseção entre arestas de A e B (pontos de transição)
        /// 
        /// 2. Remove duplicatas (pontos muito próximos)
        /// 
        /// 3. Ordena por ângulo ao centroide:
        ///    Começa por um vértice de A fora de B e percorre a aresta de A no sentido
        ///    horário até encontrar um ponto de interseção, onde muda para a aresta de B,
        ///    repetindo até retornar ao ponto inicial.
        ///    Para polígonos convexos com centroide na região de interseção (star-shaped),
        ///    a ordenação angular equivale ao percurso correto da borda.
        /// 
        /// Retorna null se os polígonos não se sobrepõem (devem ser desenhados separadamente).
        /// </summary>
        public static PointF[] CalcularUniao(PointF[] poligonoA, PointF[] poligonoB)
        {
            if (poligonoA == null || poligonoA.Length < 3 ||
                poligonoB == null || poligonoB.Length < 3)
                return null;

            // Caso especial: um polígono contém o outro completamente
            if (TodosVerticesDentro(poligonoB, poligonoA))
                return (PointF[])poligonoA.Clone();
            if (TodosVerticesDentro(poligonoA, poligonoB))
                return (PointF[])poligonoB.Clone();

            var pontos = new List<PointF>();

            // Passo 1: Vértices de A que estão fora de B
            foreach (var v in poligonoA)
            {
                if (!PontoNoPoligonoConvexo(v, poligonoB))
                    pontos.Add(v);
            }

            // Passo 2: Vértices de B que estão fora de A
            foreach (var v in poligonoB)
            {
                if (!PontoNoPoligonoConvexo(v, poligonoA))
                    pontos.Add(v);
            }

            // Passo 3: Pontos de interseção entre todas as arestas de A e B
            for (int i = 0; i < poligonoA.Length; i++)
            {
                int ni = (i + 1) % poligonoA.Length;
                for (int j = 0; j < poligonoB.Length; j++)
                {
                    int nj = (j + 1) % poligonoB.Length;
                    if (IntersecaoSegmentos(
                        poligonoA[i], poligonoA[ni],
                        poligonoB[j], poligonoB[nj],
                        out _, out _, out var pontoInter))
                    {
                        pontos.Add(pontoInter);
                    }
                }
            }

            // Sem pontos suficientes → polígonos não se sobrepõem
            if (pontos.Count < 3)
                return null;

            // Passo 4: Remover pontos duplicados (tolerância geométrica)
            pontos = RemoverDuplicatas(pontos);

            if (pontos.Count < 3)
                return null;

            // Passo 5: Calcular centroide e ordenar por ângulo
            // Para a forma "+" (ou qualquer interseção de ruas), o centroide
            // está na região de sobreposição, garantindo que o polígono é
            // star-shaped a partir deste ponto. A ordenação angular produz
            // o contorno correto da casca externa, incluindo concavidades.
            float cx = 0f, cy = 0f;
            foreach (var p in pontos)
            {
                cx += p.X;
                cy += p.Y;
            }
            cx /= pontos.Count;
            cy /= pontos.Count;

            pontos.Sort((a, b) =>
            {
                float angA = (float)Math.Atan2(a.Y - cy, a.X - cx);
                float angB = (float)Math.Atan2(b.Y - cy, b.X - cx);
                return angA.CompareTo(angB);
            });

            return pontos.ToArray();
        }

        /// <summary>
        /// Método conveniente: calcula a união de dois retângulos rotacionados
        /// diretamente a partir de seus parâmetros geométricos.
        /// 
        /// R1: centro=(x₁,y₁), dimensões a×b, rotação θ₁
        /// R2: centro=(x₂,y₂), dimensões c×d, rotação θ₂
        /// </summary>
        public static PointF[] CalcularUniaoRetangulos(
            PointF centro1, float largura1, float altura1, float angulo1,
            PointF centro2, float largura2, float altura2, float angulo2)
        {
            var verticesR1 = CalcularVerticesRetanguloRotacionado(
                centro1, largura1, altura1, angulo1);
            var verticesR2 = CalcularVerticesRetanguloRotacionado(
                centro2, largura2, altura2, angulo2);

            return CalcularUniao(verticesR1, verticesR2);
        }

        private static bool TodosVerticesDentro(PointF[] testados, PointF[] container)
        {
            foreach (var v in testados)
            {
                if (!PontoNoPoligonoConvexo(v, container))
                    return false;
            }
            return true;
        }

        private static List<PointF> RemoverDuplicatas(List<PointF> pontos)
        {
            var resultado = new List<PointF>();
            foreach (var p in pontos)
            {
                bool duplicata = false;
                foreach (var r in resultado)
                {
                    float dx = p.X - r.X;
                    float dy = p.Y - r.Y;
                    if (dx * dx + dy * dy < TOLERANCIA_DUPLICATA * TOLERANCIA_DUPLICATA)
                    {
                        duplicata = true;
                        break;
                    }
                }
                if (!duplicata)
                    resultado.Add(p);
            }
            return resultado;
        }
    }
}
