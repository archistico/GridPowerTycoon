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

### Step 23A - Cloud unlock map preview

Completed:
- selected cloud area now previews the exact cells that would be revealed;
- preview uses existing `AreaUnlockSystem` logic;
- preview shows whether unlock is currently affordable/valid;
- no gameplay logic changes.

### Step 23B - Terrain clear map preview

Completed:
- selected forest/mountain cells now show a clear preview;
- preview shows required tool type and amount;
- preview indicates whether the player has enough tools;
- no gameplay logic changes.

### Step 23C - Expansion and obstacle property clarity

Completed:
- forest/mountain properties now explain required tools and missing amount;
- cloud properties now show current resources against unlock cost;
- cloud issue text distinguishes missing money, missing research and nothing to reveal;
- no gameplay logic changes.

### Step 23D - Area unlock result summary

Completed:
- cloud unlock success messages now include a compact summary of revealed terrain types;
- status feedback now distinguishes land, forest, mountain and other tile types after unlock;
- no gameplay logic changes.

### Step 23E - Expansion system documentation

Completed:
- documented Milestone 23 expansion and obstacle feedback in `docs/EXPANSION_SYSTEM.md`;
- added expansion feedback section to `docs/FEEDBACK_SYSTEM.md`;
- no code changes.

### Step 23F - Building range overlay

Completed:
- selecting a built heat converter now shows its operational range on the map;
- covered heat producers are highlighted;
- overlay uses the same Chebyshev distance model as heat conversion;
- no gameplay logic changes.

### Step 23G - Heat coverage inverse feedback

Completed:
- selecting a heat producer now highlights converters that cover it;
- existing selected-converter range overlay remains unchanged;
- no gameplay logic changes.

### Step 23H - Heat converter placement range preview

Completed:
- heat converter building tools now show future range under the mouse;
- invalid placement range preview is shown in red/attenuated form;
- selected converter range and build preview share the same overlay helper;
- no gameplay logic changes.

### Step 23I - Milestone 23 final documentation

Completed:
- consolidated final expansion/range feedback documentation;
- documented final Milestone 23 state;
- closed Milestone 23 as a map readability milestone;
- no code changes.

Milestone 23 is now complete.

### Step 23J - Useful stability tests

Completed:
- added targeted core tests for heat range, heat operational status, cloud preview and terrain clear validation;
- avoided fragile rendering tests;
- no gameplay logic changes.

## Milestone 24 - Onboarding and early guidance

Goal: make the early game easier to understand without adding heavy tutorial flow or interrupting the player.

### Step 24A - First objective hint

Completed:
- added dynamic objective text to the status bar;
- objective is derived from world state;
- no save-data changes;
- no gameplay logic changes.

### Step 24B - Early game checklist

Completed:
- added compact early-game checklist;
- checklist is derived from world state;
- checklist hides after completion;
- no save-data changes;
- no gameplay logic changes.

### Step 24C - Contextual heat and generator hints

Completed:
- added heat-specific guidance to build tool properties;
- improved build menu support text for heat producers/converters;
- improved selected-building issue text for heat converters;
- no gameplay logic changes.

### Step 24D - Help panel / quick guide

Completed:
- added HELP button;
- added H shortcut;
- added quick guide overlay;
- no save-data changes;
- no gameplay logic changes.

### Step 24E - Help panel current objective/checklist

Completed:
- HELP panel now shows the current objective;
- HELP panel now repeats the early checklist state;
- static guide remains available as `BASICS`;
- no gameplay logic changes.

### Step 24F - Milestone 24 final documentation

Completed:
- consolidated Milestone 24 onboarding documentation;
- documented final status bar objective, early checklist and HELP behavior;
- confirmed no saved tutorial/quest state;
- no code changes.

Milestone 24 is now complete.

## Milestone 25 - Progression guidance and mid-game clarity

Goal: keep the player oriented after the first onboarding sequence, without adding blocking tutorials or saved quest state.

### Step 25A - Mid-game objective hints

Completed:
- extended objective hints after the early checklist;
- added guidance for first upgrade, research, expansion, terrain clearing, managers and next heat tiers;
- kept objective state derived from current world state;
- no save-data changes;
- no gameplay logic changes.

### Step 25B - Goal-aware HELP details

Completed:
- HELP panel now shows a `NEXT` detail line below the current objective;
- detail line explains missing resources or the recommended UI/action;
- no save-data changes;
- no gameplay logic changes.

### Step 25C - Progression bottleneck feedback

Completed:
- HELP panel now shows a `BOT` bottleneck line;
- bottleneck is derived from resource rates and world state;
- no gameplay logic changes;
- no save-data changes.

### Step 25D - Progression advisor extraction and tests

Completed:
- extracted progression guidance into Core `ProgressionAdvisor`;
- connected UI to the advisor;
- added targeted Core tests for objective/detail/bottleneck behavior;
- no save-data changes;
- no gameplay logic changes.

### Step 25E - Milestone 25 final documentation

Completed:
- consolidated Milestone 25 progression guidance documentation;
- recorded final behavior of objective, NEXT and BOT guidance;
- recorded that `ProgressionAdvisor` is now the Core owner of progression guidance;
- confirmed no saved quest/mission state;
- prepared the next milestone as content expansion / new buildings.

Milestone 25 is now complete.

## Milestone 26 - New buildings and production chains

Goal: add new buildings that introduce distinct strategic roles, not just stronger versions of existing buildings.

Planned order:
- 26A - Substation / Transformer;
- 26B - Heat sink / Cooler;
- 26C - Maintenance center;
- 26D - Tool warehouse;
- 26E - Geothermal plant;
- 26F - Data center;
- 26G - Nuclear plant or advanced reactor.

Design rule: every new building must solve a specific gameplay problem or create a new trade-off.

### 26A - Substation / Transformer

Completed:
- added first support building: Substation / Transformer;
- added global energy efficiency bonus mechanic;
- added research unlock;
- added UI/property/build-card support;
- added ResourceRateSnapshot test.

Design role: improves an already productive grid by increasing global energy output. It is not a producer and not simple storage.

### 26B - Heat sink / Raffreddatore

Completed:
- added heat sink building and research;
- added heat dissipation mechanic;
- added UI/property/map support;
- added heat-system test.

Design role: defensive heat management. It prevents heat risk without producing energy, creating a choice between conversion efficiency and safety.

### 26C - Maintenance center / Centro manutenzione

Completed:
- added maintenance building and research;
- added global lifetime-decay reduction mechanic;
- added UI/property/map support;
- added simulation test.

Design role: operational stability. It slows wear before managers fully automate renewals.

### 26D - Tool warehouse / Magazzino strumenti

Completed:
- added tool warehouse building and research;
- added dynamic axe/mine capacity bonus;
- updated tool generation caps;
- added UI/property/map support;
- added tool-generation test.

Design role: expansion logistics. It lets the player store more axes and mines before clearing terrain, without increasing generation speed.

### 26E - Geothermal plant / Centrale geotermica

Completed:
- added geothermal building and research;
- connected it to the existing heat-production chain;
- added UI build/research entries;
- added heat-system test.

Design role: stable mid-game heat source. It is not direct electricity; it requires conversion infrastructure.

### 26F - Data center

Completed:
- added data center building and research;
- connected it to existing research/energy-consumption mechanics;
- added UI build/research entries;
- added simulation tests.

Design role: major energy sink. It gives mature grids a reason to produce large surplus energy by converting it into research throughput.

### 26G-pre - New buildings consistency pass

Status: implemented.

Before adding the nuclear reactor, the Milestone 26 content received a consistency pass over runtime data and UI ids. The pass added `RuntimeDataConsistencyTests`, which loads the real JSON files used by MonoGame and checks the relationships that previously caused partial Core/UI/data drift.

Covered checks:
- every building `requiredResearchId` exists in `research.json`;
- every research `unlockBuildingIds` entry exists in `buildings.json`;
- every research `managedBuildingIds` entry exists in `buildings.json`;
- every research `requiredResearchIds` prerequisite exists in `research.json`;
- every upgrade `targetBuildingId` exists in `buildings.json`;
- every upgrade `requiredResearchId` exists in `research.json`;
- every UI build button id exists in the building catalog;
- every UI research button id exists in the research catalog;
- every building is reachable from the build UI;
- every research is reachable from the research UI.

This pass intentionally adds no gameplay feature. Its role is to make the next content step safer.

### 26G - Nuclear plant / Advanced reactor

Status: done.

Implemented content:
- `nuclear_reactor` building;
- `nuclear_power` research;
- `nuclear_heat_1` upgrade;
- `nuclear_lifetime_1` upgrade;
- build, research and upgrade UI ids;
- runtime consistency checks for upgrade UI ids;
- focused nuclear runtime-data tests.

Design result: the nuclear reactor is a high-tier heat producer, not a direct power producer. It has a 3 x 3 footprint, very high cost, very high heat output, high energy consumption and late prerequisites. It is unlocked only after the player has passed through geothermal power, maintenance and the data-center research path. This keeps it different from a simple larger coal or gas plant: the value is extreme, but the supporting infrastructure requirement is intentionally high.

### 26H - Milestone 26 final documentation and balance pass

Status: done.

Completed:
- added `docs/MILESTONE_26_BALANCE.md` with final runtime values for all Milestone 26 buildings;
- documented the heat-chain ratios for solar, coal, geothermal, gas and nuclear producers;
- recorded the current expansion-tool generation/cap values;
- closed `docs/MILESTONE_26_STATUS.md`;
- updated balance notes, game-design notes and AI handoff.

Milestone 26 is now complete. The next work should be stabilization and save/data compatibility, not another production tier.

## Milestone 27 - Save, stability and quality of life

Status: started.

Goal: stabilize persistence and quality-of-life now that many new properties and buildings have been added.

### 27A - Save compatibility check for new building properties

Status: prepared.

The save format remains at version `1`, now centralized as `SaveGame.CurrentVersion`. Step 27A confirms that the Milestone 26 building-definition properties stay data-driven and are not duplicated into the save payload:

- `EnergyEfficiencyBonus`;
- `HeatDissipationPerSecond`;
- `MaintenanceEfficiencyBonus`;
- `ToolCapacityBonus`.

The save stores only persistent world state and building ids. After restore, the active `GameData` catalog continues to provide the runtime behavior for substations, heat sinks, maintenance centers and tool warehouses. This keeps balancing changes in JSON compatible with existing saves.

Added regression coverage for:

- schema version stability;
- definition-only property exclusion from save JSON;
- restored support-building bonuses;
- 3x3 nuclear reactor footprint persistence;
- explicit failure on unknown saved building/research/upgrade ids.

`SaveGameService.RestoreWorld` now validates saved ids against the supplied `GameData` before rebuilding the world. A later migration step should turn this guard into a migration policy for renamed or removed ids.

### 27B - Save integrity validation and corrupted JSON handling

Status: prepared.

Step 27B expands the save guard from identifier compatibility to structural integrity. Saves are now rejected before restore if the map has duplicate or missing coordinates, if a building footprint is incomplete, if a tile points to a building outside its footprint, or if an upgrade level is negative or higher than the active upgrade definition allows.

Malformed JSON loaded from disk is converted to a readable save-load failure. MonoGame startup no longer crashes on a bad save file: it falls back to a new game and shows a status message. Manual load keeps the current world and reports `SAVE LOAD FAILED`.

Added `SaveIntegrityTests` for duplicate tiles, incomplete multi-tile footprints, extra tile references, over-max upgrade levels and corrupted JSON.

### 27C - Visible save/data version information

Status: prepared.

Step 27C separates save-schema versioning from runtime data-catalog versioning. `SaveGame.CurrentVersion` remains the save-format version, while `GameData.CurrentVersion` is now stored in the save payload as `DataVersion`. Restore rejects unsupported data versions explicitly, which prepares the project for a real migration policy instead of relying on implicit JSON compatibility.

MonoGame now exposes the current persistence contract in the status bar. A new session shows `SAVE V1 DATA V1 UNSAVED SESSION`; after saving or loading, the status shows the save version, data version and last save timestamp in UTC. This is intentionally small and debug-friendly: it helps verify what is loaded during manual testing without adding a full save-management screen yet.

Added tests cover version exposure, compact summary formatting and explicit unsupported-data-version failure.


### 27D - Safe handling for missing/renamed ids in old saves

Status: prepared.

Step 27D adds `SaveIdMigrationMap`, allowing old saved building, research and upgrade ids to be translated to current runtime ids during restore. The default behavior stays strict: unknown ids still fail unless a migration is explicit, and duplicate targets after migration are rejected. No current ids were renamed in this step.

### 27E - Confirm NEW and EXIT when dirty

Status: prepared.

Step 27E adds dirty-state tracking for the active session. The MonoGame status bar appends `MODIFIED` when the current run has changes after the last clean snapshot. Clean snapshots are created after load, save, new game and the offline-progress autosave path; successful gameplay actions mark the session dirty, and passive simulation time also marks it dirty after a short grace interval.

`NEW` and `EXIT` are now protected by a repeat-to-confirm flow when dirty. The first click reports `UNSAVED CHANGES - CLICK NEW AGAIN TO CONFIRM` or `UNSAVED CHANGES - CLICK EXIT AGAIN TO CONFIRM`. Repeating the same action confirms it. Exit still saves before closing after confirmation.

Added `SaveDirtyState` and `SaveDirtyStateTests` so the confirmation policy is covered in Core instead of being embedded only in UI code.

Recommended remaining order:
- 27F - Optional autosave;
- 27G - Backup save on write;
- 27H - Final persistence regression pass.

Milestone 27 should be completed before treating the game as a stable playable release candidate.

## Stop checkpoint - 2026-06-06

Current stop point:
- Milestone 26 is complete through Step 26H;
- Milestone 27 has started with Step 27A;
- local `dotnet test` was reported passing after Step 26G;
- Step 27A adds save compatibility guards and persistence regression tests;
- Step 27B adds structural save validation and safe corrupted-JSON handling;
- Step 27C adds save/data version visibility and a compact runtime status string.

Recommended next step:
`Step 27G - Backup save on write`.

## Step 27D - Migrazione sicura degli ID nei salvataggi

Stato: preparato.

Aggiunto `SaveIdMigrationMap` per tradurre vecchi id salvati verso gli id correnti di edifici, ricerche e upgrade durante il restore. Il comportamento predefinito rimane rigoroso: gli id sconosciuti continuano a fallire se non esiste una migrazione esplicita. Se due id salvati finiscono sullo stesso target dopo la migrazione, il caricamento viene bloccato con un errore chiaro per evitare stati doppi o ambigui.

Questo step non rinomina nessun dato attuale; prepara solo l'infrastruttura per futuri refactor dei cataloghi JSON.


## 2026-06-06 - Step 27F Optional autosave

Step 27F adds `AutoSaveState` in `GridPowerTycoon.Core.Save`. The class is intentionally small and testable: it accumulates elapsed time only while the game is dirty, triggers an autosave decision when the configured interval is reached, resets after a clean snapshot, and stays inert when disabled.

`Game1` now owns an autosave state configured to 60 seconds and enabled by default. After simulation and dirty-state tracking, the update loop asks the autosave state whether a save is due. When due, MonoGame writes the normal `Saves/savegame.json`, refreshes the compact save/data status string, marks the game clean and reports `AUTOSAVED`. If the write fails, it reports `AUTOSAVE FAILED` without marking the session clean.

Manual save, manual load, explicit new game and the offline-progress save path continue to go through the clean-snapshot flow and therefore reset the autosave timer. Autosave can be toggled at runtime with `F6`, which reports `AUTOSAVE ON` or `AUTOSAVE OFF`.

Added `AutoSaveStateTests` for interval behavior, clean reset, disabled behavior and toggle reset.

Next recommended step: Step 27I - Final persistence regression pass.

## 2026-06-06 - Step 27G Backup save on write

Step 27G strengthens persistence writes without changing gameplay. `SaveGameService.SaveToFile` now serializes the new save to a temporary file, preserves the previous main save as `savegame.backup.json`, and then replaces the main save. This keeps the previous known-good snapshot available whenever a later save overwrites `savegame.json`.

The backup naming rule is centralized in `SaveGameService.GetBackupPath`, so future load-fallback work can use the same path instead of rebuilding it in UI code. Because the behavior lives in Core, manual saves, autosaves and startup offline-progress saves all follow the same write policy.

Tests in `SaveGameServiceTests` verify that the first save does not invent a backup, the second save creates a backup from the previous file, the main file contains the latest snapshot, and the backup file contains the previous snapshot.

Next recommended step: Step 27I - Final persistence regression pass.


## Step 27H - Load fallback from backup

Stato: preparato.

Il sistema di salvataggio ora usa il backup generato in scrittura come reale percorso di recupero in lettura. Se il file principale non è leggibile o non può essere ripristinato, il Core tenta `savegame.backup.json`. L'integrazione MonoGame usa lo stesso percorso sia all'avvio sia con il caricamento manuale, mostrando `MAIN SAVE FAILED - BACKUP LOADED` quando il recupero avviene dal backup.


## Step 27I - Final persistence regression pass

Stato: preparato.

Questo step chiude il blocco salvataggi/stabilità con test regressivi incrociati. Non cambia il gameplay, ma verifica che le parti introdotte nel Milestone 27 lavorino correttamente insieme: rotazione del backup dopo più salvataggi, caricamento dal backup con migrazione ID attiva e comportamento dell'autosave dopo il superamento della soglia.

Dopo il passaggio dei test, il Milestone 27 può essere considerato completato. Il prossimo milestone consigliato è il 28, dedicato a feedback UX e chiarezza di gioco.

## Milestone 28 - UX and gameplay feedback

Stato: prossimo.

Obiettivo: rendere più leggibili le decisioni del gioco per il giocatore. Ora che il salvataggio è protetto da versioni, validazione, migrazione, backup, fallback e autosave, il lavoro successivo dovrebbe concentrarsi su messaggi, tooltip e pannelli che spiegano perché una costruzione, una ricerca o un upgrade sono disponibili o bloccati.

Primo step consigliato:

```text
Milestone 28A - Better build/research/upgrade feedback messages
```


## Step 27J - Top menu HELP/EXIT hit-test cleanup

Before entering Milestone 28, the top menu received a small UX consistency fix. The help action now lives on the visible HELP button after UPGRADE, and the invisible HELP hit-test area near EXIT has been removed. The right-side command group now contains only NEW, LOAD, SAVE, VIEW and EXIT.

Next recommended step:

```text
Milestone 28A - Better build/research/upgrade feedback messages
```

## Step 28A - Better build/research/upgrade feedback messages

Stato: preparato.

Aggiunto `GameplayFeedbackFormatter` nel Core per trasformare gli errori di build, research e upgrade in messaggi più utili per il giocatore. La UI non mostra più solo messaggi generici come `NEED MONEY` o `MISSING PREREQUISITE`: quando possibile indica nome dell'edificio, ricerca o upgrade, costo, risorsa disponibile, quantità mancante e prerequisito richiesto.

`ResearchResult` e `UpgradeResult` mantengono ora l'id tentato anche in caso di fallimento. Questo permette alla status bar di spiegare quale ricerca o upgrade è bloccato senza dover duplicare logica nella UI.

Aggiunti test dedicati in `GameplayFeedbackFormatterTests`. Il gameplay e il bilanciamento non cambiano.

Prossimo step consigliato:

```text
Milestone 28B - Tooltip details for locked/available cards
```


## Milestone 28A - Better build/research/upgrade feedback messages

Stato: completato.

Aggiunto `GameplayFeedbackFormatter` per trasformare errori di build, ricerca e upgrade in messaggi più concreti nella status bar, con costi, risorse disponibili e prerequisiti mancanti.

## Milestone 28B - Hover details for locked/available cards

Stato: preparato.

Le card BUILD, RESEARCH e UPGRADE ora mostrano un pannello flottante di dettaglio al passaggio del mouse. Il pannello riassume stato, costo, prerequisiti, effetto, livello e target in base al tipo di card. La logica dei testi resta nel Core tramite `GameplayFeedbackFormatter`, mentre MonoGame si limita a disegnare il pannello.

Prossimo step consigliato: Milestone 28C - Critical resource warnings.


## Milestone 28D - Production/consumption summary panel

Prepared. Adds a read-only grid summary panel for energy production/consumption/net, research and money rate, heat produced/managed/free, maintenance risk and tool stock/rates. The summary is formatted in Core through `GameplayFeedbackFormatter` and drawn by MonoGame only when there is enough safe central screen space.

Next recommended step: Milestone 28E - Building details panel consistency.


## Milestone 28E - Building details panel consistency

Stato: preparato.

Il pannello proprietà a destra usa ora un ordine stabile di righe per edifici, terreno, nuvole, celle vuote e preview del build tool. I valori non applicabili rimangono `-`, mentre i dettagli edificio sono aggregati in righe compatte per produzione, consumi, storage, calore, manutenzione e vita utile. La modifica è solo UI e non cambia gameplay, bilanciamento o salvataggi.


## Milestone 28F - Locked reason visibility pass

Status: prepared. Build, research and upgrade card availability is now formatted by Core-side `GameplayFeedbackFormatter` helpers so visible card status and hover details share the same concrete lock/missing-resource reason.

## Milestone 28G - Milestone 28 closure and feedback regression documentation

Stato: preparato.

Questo step chiude il Milestone 28 con documentazione finale e regressioni mirate. Aggiunge `docs/MILESTONE_28_FEEDBACK.md` come contratto del sistema feedback e rafforza `GameplayFeedbackFormatterTests` per evitare divergenze future tra stato visibile delle card, hover details, stati completati/maxed e formato del pannello riepilogo produzione.

Non cambia gameplay, economia, bilanciamento, salvataggi o rendering operativo. Dopo il passaggio dei test, il Milestone 28 può essere considerato completato.

Prossimo milestone consigliato:

```text
Milestone 29 - Gameplay flow and progression polish
```
