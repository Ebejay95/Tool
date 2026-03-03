// IModule und ModuleOrderAttribute wurden nach ServerKernel verschoben.
// Sie gehören nicht in SharedKernel (WASM-kompatibel), da sie
// IServiceCollection/IConfiguration/IHostEnvironment verwenden – reine Server-Typen.
