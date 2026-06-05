# Roadmap

## Step 01 — Solution e base tecnica

Stato: completato.

La solution contiene `GridPowerTycoon.Core`, `GridPowerTycoon.MonoGame` e `GridPowerTycoon.Core.Tests`. La mappa, gli edifici e l'economia sono separati dalla parte grafica.

## Step 02 — BuildSystem Core

Stato: completato.

Il Core permette di costruire edifici sulla mappa tramite `BuildSystem`, con controllo su denaro, celle fuori mappa, celle non edificabili e celle occupate. Le batterie applicano immediatamente l'aumento di capacità energetica.

## Step 03 — Collegamento BuildSystem a MonoGame

Stato: completato in questa modifica.

La schermata MonoGame ora permette di selezionare gli edifici iniziali dal menu laterale o con i tasti `1`, `2`, `3`, `4`. Il click sulla griglia chiama `BuildSystem.Build(...)`. La mappa mostra gli edifici costruiti, evidenzia la cella sotto il mouse e mostra una preview verde/rossa in base alla possibilità di costruire.

## Prossimo step — Produzione, vendita e vita edifici

Il prossimo blocco deve introdurre la simulazione base: produzione energia, produzione ricerca, scadenza edifici, vendita manuale e vendita automatica degli uffici.

## Step 04 — Simulazione base produzione/vendita

Stato: preparato.

Questo step introduce la prima simulazione economica continua: gli edifici attivi producono energia e ricerca, gli edifici con durata residua scadono, il tasto SELL vende tutta l'energia accumulata e gli uffici vendono automaticamente una quota di energia al secondo. I valori continuano a provenire da JSON tramite `BuildingDefinition` ed `EconomySettings`.


## Fix JSON enum converter
- Added JsonStringEnumConverter to GameDataLoader so JSON string enum values such as building category "PowerProducer" deserialize correctly.
- Added regression test for loading building categories from JSON strings.
