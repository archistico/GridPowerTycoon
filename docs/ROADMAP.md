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

## Step 18E - Demolizione manuale edifici

Stato: preparato.

Il pannello proprietà permette ora di demolire manualmente un edificio selezionato. La demolizione non dà rimborso nella versione attuale: serve soprattutto per correggere layout, liberare celle e sostituire manualmente impianti non più utili. Se l'edificio contribuiva alla capacità batteria, la capacità massima viene ridotta e l'energia accumulata viene ridotta al nuovo massimo se necessario.

## Step 18F - Feedback fondi insufficienti

Stato: preparato.

I pulsanti `REPLACE` e `RESTORE` sono ora disabilitati quando il giocatore non ha denaro sufficiente. Il pannello proprietà mostra chiaramente che servono fondi, invece di lasciare cliccare un'azione destinata a fallire.

Per la costruzione di nuovi edifici è stato aggiunto un feedback visivo diretto sulla mappa: se il click fallisce per `NotEnoughMoney`, sulla cella compare un marker rosso con `$` barrato, mentre la status bar mantiene il messaggio tecnico del fallimento. Anche il menu BUILD evidenzia i costi non finanziabili con `NEED $...`.

## Step 18G - Conferma DEMOLISH e proprietà celle vuote

Stato: preparato.

Il pulsante `DEMOLISH` ora richiede conferma a due click ed è stato spostato in alto nel pannello proprietà dell'edificio, separandolo visivamente da `REPLACE`/`RESTORE`. La conferma viene annullata quando il giocatore clicca sulla mappa o seleziona un altro edificio.

La modalità build non è più preselezionata all'avvio e viene disattivata quando si clicca un edificio già costruito. Le celle vuote hanno ora un pannello proprietà leggibile, inclusa la distinzione `Plain` / `FREE / BUILDABLE`. La riga `HEAT CONV` è stata rinominata in `HEAT TO ENERGY` per chiarire la conversione da calore a energia.


## Step 18H - HEAT IN nel properties panel

Stato: preparato.

Il pannello proprietà ora distingue anche `HEAT IN`, utile soprattutto per i convertitori di calore. La riga indica quanto calore l'edificio può assorbire al secondo e il raggio operativo, mentre `HEAT TO ENERGY` mostra solo l'energia generata dalla conversione.


## Step 18I - Testi HEAT IN più leggibili

Rimosse le abbreviazioni poco chiare nel properties panel dei convertitori di calore. Il raggio non è più mostrato come `R3`, ma come `RANGE 3 CELLS`; la conversione usa testi espliciti come `ABSORBS .../S` e `PRODUCES .../S ENERGY`.

## 2026-06-06 - Step 18J: build tool cancel e dettagli strumento

Aggiunta una gestione più esplicita del tool BUILD attivo. Il click destro annulla lo strumento di costruzione, mentre cliccare di nuovo lo stesso pulsante BUILD o premere di nuovo lo stesso tasto numerico lo disattiva. La status bar ora ricorda l'uso `LEFT CLICK BUILD, RIGHT CLICK CANCEL` e il pannello proprietà mostra categoria, nome, costo e dimensione del tool selezionato anche quando non è ancora stata scelta una cella.

## 2026-06-06 - Step 18K: riepilogo economico properties panel

Stato: preparato.

Il pannello proprietà ora mostra informazioni economiche più utili per decidere cosa costruire o potenziare: costo di costruzione, prossimo upgrade, denaro stimato al secondo, bilancio netto energia e payback stimato. La stessa logica viene usata anche quando è attivo un tool di costruzione, così il giocatore può valutare l'edificio prima di piazzarlo.

## 2026-06-06 - Step 18L: leggibilità LIFE/PAYBACK e righe proprietà

Stato: preparato.

Rifinito il pannello proprietà: `LIFE` mostra `50 S / 300 S`, `PAYBACK` evita simboli non supportati dal font e abbreviazioni poco chiare, mentre l'alternanza grafica delle righe è ora calcolata sull'indice della riga e non sulla posizione verticale.

## 2026-06-06 - Step 18M: pannello sinistro più informativo e bottoni uniformi

Stato: preparato.

Le colonne `BUILD`, `RESEARCH` e `UPGRADE` hanno ora pulsanti della stessa altezza. I pulsanti BUILD espongono subito costo, bilancio energia, impatto sul calore e descrizione minima dell'edificio. I pulsanti RESEARCH indicano costo/stato, cosa viene sbloccato o gestito e una descrizione breve. Gli UPGRADE mantengono lo stile già approvato, ma sono stati allineati alla stessa altezza degli altri pulsanti.

## 2026-06-06 - Step 18N: glyph mancanti nel font pixel

Stato: preparato.

Corretto il renderer testo pixel interno: apostrofi, apostrofo tipografico, lettere accentate italiane e alcuni separatori usati nei testi della UI non vengono più mostrati come `?`. Il gioco non usa un font esterno, ma una tabella di glifi 5x7 in `PixelTextRenderer`; la tabella è stata estesa per supportare meglio i testi italiani nel pannello sinistro e nel pannello proprietà.

## 2026-06-06 - Step 19A: normalizzazione pannello sinistro

Stato: preparato.

Inizio Milestone 19. BUILD, RESEARCH e UPGRADE mantengono pulsanti uniformi, ma ora usano anche una griglia interna comune per le quattro righe informative. BUILD è stato riallineato allo stile più chiaro degli UPGRADE: niente icona grande che sottrae spazio al testo, barra verticale di categoria, costo/requisito più leggibile, energia netta, calore e descrizione. RESEARCH indica il prerequisito specifico mancante quando possibile.

## 2026-06-06 - Step 19B: colonne sinistre più larghe e titoli più leggibili

Stato: preparato.

Prosegue la Milestone 19 sulla leggibilità UI. Il pannello laterale sinistro è stato allargato e i titoli dei pulsanti BUILD/RESEARCH/UPGRADE sono ora più grandi. In BUILD è stata rimossa la numerazione davanti al nome edificio, così la prima riga mostra direttamente il nome leggibile della costruzione. Anche le etichette delle risorse nella barra superiore sono state rese più visibili.

## 2026-06-06 - Step 19C: stati leggibili nel pannello sinistro

Stato: preparato.

BUILD, RESEARCH e UPGRADE ora mostrano badge sintetici di disponibilità nella prima riga del pulsante. La riga costo/stato è stata resa più descrittiva, così il pannello sinistro comunica immediatamente se un elemento è pronto, attivo, completato, bloccato o non acquistabile per mancanza di risorse.

## 2026-06-06 - Step 19D: descrizioni operative nel pannello sinistro

Stato: preparato.

Prosegue la Milestone 19. I pulsanti BUILD ora usano una riga di effetto principale più esplicita: produzione energia, produzione calore, conversione calore, storage, ricerca o vendita automatica vengono descritti direttamente con verbo e quantità. La quarta riga non ripete più soltanto la descrizione lunga del catalogo, ma fornisce un'indicazione operativa breve, per esempio se un generatore ha bisogno di calore vicino, se un produttore di calore richiede un generatore o se una batteria serve a evitare sprechi di energia.

Anche RESEARCH e UPGRADE sono stati resi più operativi: le ricerche distinguono meglio nuovi edifici e automazioni, mentre gli upgrade mostrano chiaramente il target dell'effetto. Lo scopo è far capire il valore pratico di ogni pulsante senza dover aprire il pannello proprietà.

## 2026-06-06 - Step 19E: purpose nel properties panel

Stato: preparato.

Aggiunta una riga `PURPOSE` nel pannello proprietà. La riga spiega in modo breve il ruolo dell'edificio selezionato, del tool di costruzione attivo o della cella/terreno selezionato. Questo completa il collegamento tra pannello sinistro e properties panel: il pannello sinistro spiega cosa si può scegliere, il properties panel spiega cosa si sta guardando e perché è rilevante.


## 2026-06-06 - Step 19F: single left panel mode

Stato: preparato.

Il pannello laterale sinistro non mostra più tre colonne contemporaneamente. È stata introdotta una fascia di navigazione con `BUILD`, `RESEARCH` e `UPGRADE`; solo la sezione attiva viene disegnata nella colonna sinistra unica. I comandi `NEW`, `LOAD`, `SAVE` ed `EXIT` sono stati spostati nella stessa fascia, liberando la status bar e preparando spazio futuro per `STATS`, `HELP` e `SETTINGS`.

La colonna unica usa più larghezza per testi e nomi, mentre i badge duplicati sono stati rimossi: lo stato resta espresso nella seconda riga del pulsante e il tool BUILD attivo è riconoscibile dal bordo giallo.

## 2026-06-06 - Step 19G: BUILD column full-width cards
The single-column left panel now gives BUILD cards more width and height. Building names can be displayed with fewer abbreviations, cards show state/cost, main effect, operational note, and a final detail row with size/category. Next UI passes should apply the same full-width treatment to RESEARCH and UPGRADE cards.

## 2026-06-06 - Step 19H: RESEARCH column full-width cards

Stato: preparato.

La colonna `RESEARCH` usa ora schede più informative nella nuova interfaccia a colonna singola. Ogni ricerca mostra titolo, stato/costo, effetto principale, descrizione operativa e dettaglio sugli edifici sbloccati o gestiti. Prossimo passaggio consigliato: applicare lo stesso trattamento alla sezione `UPGRADE`.

### Step 19I - UPGRADE full-width cards

Completed:
- full-width upgrade cards aligned with the new single-column left panel;
- clearer lines for status/cost, effect, target and level;
- effect-colored accent bars for faster visual scanning;
- no gameplay or balance changes.

### Step 19J - Left panel clipping and full-width status bar

Completed:
- fixed left panel scroll clipping for full-width cards;
- prevented scrolled research/upgrade cards from invading the top resource area;
- changed the status bar to span the full window width;
- kept properties panel above the status bar.

### Step 19J Fix1 - Top mask draw order and removed scroll hint

Completed:
- redrew top resource bar and tab bar after the scrollable left list to prevent visual overlap;
- removed the `MORE/TOP/SCROLL` label from the tab/list gap;
- retained the full-width status bar.

### Step 19J Fix2 - StatusBarHeight compile fix

Completed:
- added the missing `StatusBarHeight` constant required by the full-width status bar layout.

### Step 19J Fix3 - Card-aligned left panel scrolling

Completed:
- snapped left panel scroll offsets to whole card positions;
- fixed the top empty gap caused by hiding partially visible cards;
- retained clipping and status bar fixes.

### Step 19K - Left panel card state polish

Completed:
- clearer card state line for active build tools;
- clearer `NEED MONEY` build text;
- distinct status colors for research and upgrade states;
- no gameplay changes.

### Step 19L - Properties panel readability polish

Completed:
- clearer display labels for properties;
- wider value column;
- subtle group separators;
- group-colored labels;
- no gameplay changes.

### Step 19M - Command strip future sections and status alignment

Completed:
- prepared disabled `STATS`, `HELP`, `SETTINGS` placeholders in the command strip;
- prevented future placeholders from overlapping save/load/new/exit commands;
- removed old `VIEW` strip residue;
- aligned status text from the status bar rectangle.

### Step 19M Fix1 - Restore VIEW command

Completed:
- restored `VIEW` in the command strip;
- restored the toggle fullscreen button rectangle;
- updated command-strip width calculation to include `VIEW`.

### Step 19N - Responsive command strip placeholders

Completed:
- future command placeholders now appear progressively based on available width;
- command buttons keep a slightly safer gap from the properties panel area;
- no gameplay changes.

### Step 20A - Base building roles and first balance pass

Completed:
- first building balance pass;
- clearer role for every current building;
- starting economy adjusted from `$1` first action to `$10` first action;
- early, mid and industrial numbers reduced into a more readable progression;
- balance notes updated.

### Step 20B - Research and upgrade balance alignment

Completed:
- research costs aligned to the new building economy;
- upgrade costs aligned to early/mid/industrial tiers;
- repeated upgrade multipliers reduced to avoid runaway growth;
- balance notes updated.

### Step 20C - Heat, explosion and conversion balance

Completed:
- heat warning/explosion thresholds increased;
- solar/generator, coal/medium generator and gas thermal balance adjusted;
- heat/conversion upgrade multipliers reduced;
- properties panel now includes `HEAT RISK`;
- balance notes updated.

### Step 20D - Expansion, tools and map unlock pacing

Completed:
- tool generation made faster;
- tool caps increased;
- forest clearing made earlier;
- cloud unlock shifted toward a mixed money/research investment;
- balance notes updated.

### Step 20E - Early and mid-game progression pacing

Completed:
- first research, battery and automation made earlier;
- solar + small generator setup made easier to reach;
- coal + medium generator mid-game tier brought closer;
- research and upgrade prices aligned to the revised progression;
- balance notes updated.

### Step 21A - Clear operational issue text

Completed:
- selected building properties now include `ISSUE`;
- common operational states now have actionable explanations;
- no gameplay logic changes.

### Step 21B - Map operational state badges

Completed:
- added map badges for `NO ENERGY`, `NO HEAT CONVERSION`, `HEAT WARNING`, `EXPIRED`, and `EXPLODED`;
- map badges and properties panel now rely on the same operational state calculation;
- no gameplay logic changes.

### Step 21C - Status bar badge legend

Completed:
- added responsive status bar legend for map badges;
- kept legend hidden on narrow windows;
- no gameplay logic changes.

### Step 21D - Clear failure messages

Completed:
- action failure messages now use readable, actionable text;
- raw enum names are no longer shown in the status bar for common failures;
- no gameplay logic changes.

### Step 21E - Status bar selected building feedback

Completed:
- status bar now shows selected building operational summary when no higher-priority action message is active;
- critical selected-building states are visible without reading the properties panel;
- status bar added to UI hit-test area;
- no gameplay logic changes.

### Step 21F - Feedback system documentation

Completed:
- documented Milestone 21 feedback rules in `docs/FEEDBACK_SYSTEM.md`;
- recorded badge meanings, `ISSUE` semantics, heat feedback and status bar priority;
- no code changes.

### Step 22A - Manager visibility in properties

Completed:
- properties panel now shows which manager research controls a building;
- managed and not-yet-managed states are easier to distinguish;
- no gameplay logic changes.

### Step 22B - Manager renewal feedback

Completed:
- status bar now reports automatic manager renewals;
- status bar now reports managed expired buildings that cannot be renewed due to missing money;
- repeated identical manager messages are throttled;
- no gameplay logic changes.

### Step 22C - Manager map badge

Completed:
- managed buildings now show a compact `M` badge on the map;
- manager badge remains separate from operational problem badges;
- no gameplay logic changes.

### Step 22D - Manager research impact text

Completed:
- manager research cards now show how many built structures they will manage or already manage;
- expired covered buildings are reported directly on the manager research card;
- no gameplay logic changes.
