using BenchmarkDotNet.Attributes;
using CrimeSketcher.Core;
using System.Drawing;
using Microsoft.VSDiagnostics;

namespace CrimeSketcher.Benchmarks;
[CPUUsageDiagnoser]
public class GridManagerDesenharBenchmark
{
    private GridManager _grid = null!;
    private Bitmap _bitmap = null!;
    private Graphics _graphics = null!;
    private RectangleF _areaVisivel;
    [GlobalSetup]
    public void Setup()
    {
        var scale = new ScaleManager
        {
            ZoomLevel = 1.0f
        };
        _grid = new GridManager(scale)
        {
            Visivel = true,
            EspacamentoPixels = 10f,
            SubdivisoesPrincipais = 5
        };
        _bitmap = new Bitmap(1920, 1080);
        _graphics = Graphics.FromImage(_bitmap);
        _areaVisivel = new RectangleF(0, 0, 1920, 1080);
    }

    [Benchmark]
    public void DesenharGrade()
    {
        _grid.Desenhar(_graphics, _areaVisivel);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _graphics.Dispose();
        _bitmap.Dispose();
    }
}