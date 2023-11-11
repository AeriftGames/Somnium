using Godot;
using System;

public partial class MasterSignals : Node
{
    // Game
    [Signal] public delegate void GameStartEventHandler();

    // Benchmark
    [Signal] public delegate void BenchmarkFinishPressetEventHandler();
    [Signal] public delegate void BenchmarkServerStatusEventHandler(bool newResult);
}
