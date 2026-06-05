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
