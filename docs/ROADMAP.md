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

## Step 06 - Ricerca e sblocco edifici

Stato: implementato.

- `research.json` caricato all'avvio.
- Catalogo e stato ricerca separati nel Core.
- Sistema ricerca con controlli: ricerca sconosciuta, già completata, prerequisiti mancanti, punti ricerca insufficienti.
- `BuildSystem` rispetta `requiredResearchId` degli edifici.
- UI minima per acquistare ricerche e visualizzare edifici bloccati.

## Step 06 UI polish - Barre vita e valori stabili

Stato: implementato.

- Gli edifici con `LifetimeSeconds > 0` mostrano ora una barretta di vita nella parte bassa del riquadro edificio.
- La barra diventa verde/gialla/rossa in base alla durata residua.
- Gli edifici scaduti mostrano la barra vuota e mantengono la diagonale di stato scaduto.
- I valori della TopBar (`ENERGY`, `RESEARCH`, `MONEY`) usano sempre due decimali per evitare oscillazioni visive quando cambiano velocemente.

## Step 06 UI polish - Resource rates in top bar

Added per-second resource rate display below the main top bar counters.
Energy rate is computed as an estimated net change for the next second, taking into account active producers, max storage cap and automatic selling. Research rate is the sum of active research buildings. Money rate is the estimated automatic-sale income per second.

## Step 07 - Calore e generatori

Stato: implementato in questo pacchetto.

Il gioco ora legge `Data/heat.json` e usa `HeatSystem` per accumulare calore sugli edifici produttori, convertirlo in energia tramite generatori entro raggio configurato e far esplodere gli edifici che superano la soglia di sicurezza. La mappa mostra una barra calore nella parte alta degli edifici che generano o accumulano calore; il pannello edificio mostra calore prodotto, conversione, raggio e calore accumulato.

## Step 08 - Tools e pulizia ostacoli naturali

Stato: implementato.

Aggiunta la gestione configurabile di asce e mine tramite `Data/tools.json`. Il gioco genera strumenti nel tempo, mostra i contatori nella barra superiore e permette di selezionare boschi/montagne sulla mappa. Il pannello terreno consente di disboscare un bosco spendendo asce o spianare una montagna spendendo mine; la cella diventa terreno libero e quindi edificabile.


## Step 09A - Bilanciamento strumenti

Stato: implementato.

Rallentata la generazione di asce e mine in `Data/tools.json`: ora le asce crescono a `0.005/s` e le mine a `0.0025/s`. Con i costi attuali, rimuovere un bosco richiede circa 13 minuti e 20 secondi di accumulo, mentre rimuovere una montagna richiede circa 26 minuti e 40 secondi. Aggiunto `docs/BALANCE_NOTES.md` per tracciare le decisioni di bilanciamento.

## Step 10 - Upgrade da JSON

Stato: preparato.

Obiettivo: introdurre un primo sistema di upgrade esterno ai sorgenti, configurabile da `Data/upgrades.json`.

Incluso:
- `UpgradeDefinition`, `UpgradeCatalog`, `UpgradeState`, `UpgradeSystem`;
- acquisto upgrade con costo in denaro e/o ricerca;
- controllo ricerca richiesta e livello massimo;
- moltiplicatori per energia, durata, ricerca, batterie, vendita automatica, generatori, asce e mine;
- menu upgrade minimale in MonoGame;
- test Core per acquisto upgrade e applicazione dei moltiplicatori.

Nota: gli upgrade di produzione, ricerca, vendita automatica, generatori e strumenti agiscono subito sui calcoli. L'upgrade capacità batterie ricalcola anche la capacità massima esistente. L'upgrade durata vita vale per nuovi edifici e rimpiazzi successivi.

## Step 10B - UI polish: tre colonne e dettagli completi

Completato:
- menu sinistro riorganizzato in tre colonne: BUILD, RESEARCH, UPGRADE;
- pannello upgrade spostato dalla destra al menu sinistro;
- upgrade descritti con effetto e percentuale;
- top bar uniformata: Energy, Research, Money, Axes e Mines hanno tutti valore principale e crescita al secondo;
- pannello edificio più completo: costo, dimensione, vita, output, input, capacità e conversioni;
- vita mostrata in secondi interi;
- conversione generatori esplicitata come HEAT IN ed ENERGY OUT;
- finestra MonoGame ridimensionabile.

## Step 10C - Top bar energy fill bar

- Reintroduced the compact energy fill bar next to the SELL button after the three-column UI polish.
- The bar shows current stored energy as a percentage of max battery capacity and keeps the SELL action visually connected to available energy.

## Step 10D - Consumo energia operativo e fullscreen

Stato: implementato in questo pacchetto.

- Aggiunto `energyConsumptionPerSecond` alle definizioni edificio.
- La pala eolica ora aggiunge una piccola capacità batteria (`batteryCapacity: 10`) quando viene costruita.
- Ufficio piccolo e centro ricerca piccolo consumano energia per funzionare.
- Gli edifici consumatori saltano il proprio output se non c'è energia sufficiente per coprire il consumo del tick.
- La TopBar e i rate stimati tengono conto del consumo energetico operativo.
- Il pannello edificio mostra `ENERGY IN` oltre agli output/costi.
- Il gioco parte in fullscreen borderless mantenendo la finestra ridimensionabile quando non è fullscreen.

## Step 11 - Sblocco aree coperte da nuvole

Stato: implementato in pacchetto Step 11.

- Aggiunto `Data/area-unlock.json` per costi di sblocco.
- Aggiunto supporto `hiddenRows` in `default-map.json`.
- Le celle visibili come `C` restano nuvole fino allo sblocco.
- Il Core rivela il terreno reale configurato sotto la nuvola.
- UI: clic su nuvola, pannello area bloccata, pulsante UNLOCK.

## Step 11B - Mappa arcipelago più ampia
- La mappa predefinita è stata portata da 20x12 a 64x40 celle.
- Il file `Data/maps/default-map.json` ora rappresenta un arcipelago con più isole e coste più irregolari/naturali.
- L'isola iniziale resta visibile; le isole successive sono coperte da nuvole e usano `hiddenRows` per rivelare terreno, boschi e montagne allo sblocco.
- La modifica è solo dati/documentazione: non cambia sistemi Core, input o rendering.

## Step 11C - Mappa grande centrata e zoom limitato
Stato: preparato.

Modifiche:
- mappa default portata a 128x80 celle, cioè 4x l'area della 64x40;
- isola iniziale visibile al centro della mappa;
- isole future coperte da nuvole con terreno reale in hiddenRows;
- zoom minimo aumentato a 0.75 per obbligare l'esplorazione tramite pan;
- camera iniziale centrata sull'isola/mappa;
- capacità batteria della pala eolica ridotta da 10 a 5.

## Step 12 - Salvataggio e caricamento partita

Stato: preparato.

Aggiunto sistema di salvataggio JSON in `GridPowerTycoon.Core.Save`.

Il salvataggio conserva:
- risorse: energia, capacità massima, ricerca, denaro, asce, mine;
- mappa completa: tipo cella, eventuale terreno coperto sotto le nuvole, edificio presente;
- edifici: id istanza, definizione, posizione, vita residua, calore accumulato, stato Active/Expired/Exploded;
- ricerche completate;
- livelli upgrade acquistati.

MonoGame ora usa `Saves/savegame.json` nella cartella di output. Se il file esiste viene caricato all'avvio; con F5 si salva manualmente, con F9 si ricarica, con ESC il gioco salva prima di uscire.


## Step 12A - Durata pannelli solari

Stato: completato. Il valore `lifetimeSeconds` del pannello solare in `buildings.json` è stato aumentato da 30 a 180 secondi.

## Step 13 - Offline progress
Status: implemented.

When a save exists at startup, the game computes the elapsed time since `SavedAt`, caps it with `economy.json` / `maxOfflineSeconds`, applies production, research, auto-selling, lifetime decay and tool generation, then shows a compact summary in the status bar. Heat conversion is applied offline, but explosions are disabled while the player is absent.


## Step 13 fix - OfflineProgressResult None

Fixed `OfflineProgressResult.None` constructor arguments after adding `BuildingsExploded`, so the Core project compiles with the offline progress model.

## Step 14 - Cloud area group unlock

Status: prepared.

Cloud unlocking now reveals a small connected group instead of a single tile. The action still starts from one cloud tile, but the system uses `cloudUnlockRadius` and `maxCloudTilesPerUnlock` from `Data/area-unlock.json` to reveal a bounded group of connected cloud cells. This makes expansion on the large 128x80 map less repetitive while keeping the cost and pace configurable.

## Step 15 - Stato operativo edifici più chiaro

Aggiunto uno stato operativo derivato per gli edifici selezionati, distinto dallo stato persistente `Active/Expired/Exploded`. Il pannello edificio ora può mostrare condizioni come `NO ENERGY`, `HEAT WARNING` e `NO HEAT CONVERSION`, oltre ai valori effettivi correnti e ai valori lordi quando un edificio è fermo. Questo rende più chiaro perché un edificio produce, non produce, consuma, vende o accumula calore.
## Step 16 - Primi edifici mid-game

Aggiunto il primo blocco di progressione industriale configurato nei JSON:

- Centrale a carbone: 155k$, 680 calore/s, vita 300s.
- Ufficio grande: 150k$, vende 200 energia/s e consuma energia operativa.
- Generatore medio: 100k$, converte 1k calore/s, raggio 1.
- Centrale a gas: 7.5M$, 25k calore/s, vita 300s.
- Centro ricerca grande: 10M$, produce 100 ricerca/s e consuma energia operativa.

Aggiunte ricerche collegate e primi upgrade specifici. La UI BUILD/RESEARCH/UPGRADE include i nuovi elementi. La formattazione numerica della UI ora usa prefissi SI: k, M, G, T, P, E, Z, Y.


## Step 17 - Upgrade multi-livello

Stato: preparato.

Gli upgrade non sono più limitati al comportamento “comprato una volta”. Ogni `UpgradeDefinition` può ora avere `maxLevel` maggiore di 1 e `costGrowthMultiplier`, che controlla il costo del livello successivo. La UI mostra livello corrente, livello massimo, effetto e costo NEXT.

Il sistema usa il livello corrente per applicare il moltiplicatore come potenza: un upgrade `1.5` al livello 3 produce un moltiplicatore effettivo `1.5^3`. Il costo del prossimo livello cresce con `costGrowthMultiplier^currentLevel`.

Sono stati aggiunti upgrade di produzione calore per pannello solare, carbone e gas. Le produzioni di calore ora usano `UpgradeCalculator.GetHeatPerSecond`, quindi gli upgrade di produzione calore hanno effetto su simulazione, riepilogo risorse e pannello edificio.

## Step 18 - Gestori automatici

Stato: implementato.

Aggiunto il sistema dei gestori automatici collegati alle ricerche. Una ricerca può ora indicare `managedBuildingIds`; quando è completata, gli edifici scaduti di quei tipi vengono rinnovati automaticamente se il giocatore ha abbastanza denaro. La prima implementazione rinnova solo edifici `Expired`, non edifici `Exploded`.

Gestori iniziali:

- Gestore pale eoliche
- Gestore pannelli solari
- Gestore carbone
- Gestore gas

Il rinnovo automatico usa il costo base dell'edificio e la durata effettiva aggiornata dagli upgrade multi-livello.

### Step 17A - Upgrade cost growth balancing

Stato: completato. Aumentati i `costGrowthMultiplier` in `upgrades.json` per rendere gli upgrade multi-livello più costosi e strategici ai livelli successivi.


## Step 18A - Restore edifici esplosi

- Corretto il ripristino manuale: il pulsante di ripristino ora è disponibile anche per edifici in stato `Exploded`.
- Il ripristino manuale paga il costo dell'edificio, azzera il calore accumulato e riporta lo stato ad `Active`.
- I gestori automatici continuano a non rinnovare edifici esplosi: il ripristino degli esplosi resta una scelta manuale del giocatore.

## Step 18B - Fixed properties panel and persistent tile selection

- Added a fixed properties column on the right side of the screen.
- The clicked map cell remains selected and highlighted until another cell is selected.
- Building, terrain and cloud details now use the same property rows, so values stay in predictable positions across different selections.
- Context actions (REPLACE, RESTORE, CLEAR, UNLOCK) are shown in the fixed properties column.

## Step 18C - Scroll nelle colonne BUILD/RESEARCH/UPGRADE

Stato: preparato.

Le tre colonne laterali ora hanno scroll indipendente tramite rotella del mouse. La rotella sopra BUILD, RESEARCH o UPGRADE scorre solo quella colonna e non modifica lo zoom della mappa. I bottoni fuori dall'area visibile non sono cliccabili. La camera ignora zoom e pan del mouse quando il puntatore è sopra la UI, mantenendo separati input mappa e input interfaccia.

## Step 18D - Game command buttons

Status: prepared.

Added visible command buttons in the bottom status bar so the game no longer depends only on keyboard shortcuts:

- SAVE
- LOAD
- NEW
- VIEW
- EXIT

The old keyboard shortcuts remain available: F5 saves, F9 loads, ESC saves and exits. VIEW toggles fullscreen/windowed mode. NEW starts a fresh world from the current JSON data without immediately overwriting the save file.
