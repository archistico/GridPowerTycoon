# GridPower Tycoon

GridPower Tycoon è un gestionale/strategico 2D top-down a griglia sviluppato in C# con MonoGame.

La direzione tecnica è volutamente simile a quella usata per OpenCad2D: dominio separato, logica testabile, frontend grafico come strato esterno, dati di bilanciamento in JSON modificabili senza ricompilare.

## Struttura

```text
src/GridPowerTycoon.Core       Logica del gioco, mappa, economia, edifici, dati
src/GridPowerTycoon.MonoGame   Rendering, input, camera, UI, asset e JSON runtime
tests/GridPowerTycoon.Core.Tests
docs
```

## Comandi locali

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/GridPowerTycoon.MonoGame/GridPowerTycoon.MonoGame.csproj
```

## Nota

I valori di edifici ed economia sono in `src/GridPowerTycoon.MonoGame/Data` e vengono copiati nell'output di compilazione.
