# Balance notes

Questo documento raccoglie le decisioni di bilanciamento provvisorie. I valori numerici principali devono rimanere nei file JSON sotto `src/GridPowerTycoon.MonoGame/Data`, così possono essere modificati senza ricompilare.

## Strumenti: asce e mine

Le asce e le mine servono a liberare terreno edificabile e non devono crescere troppo velocemente. Se vengono generate in pochi secondi, boschi e montagne smettono di essere una scelta strategica e diventano solo un piccolo fastidio.

Valori attuali in `tools.json`:

```json
{
  "axesPerSecond": 0.005,
  "minesPerSecond": 0.0025,
  "forestClearAxesCost": 4,
  "mountainClearMinesCost": 4
}
```

Questo equivale a circa:

- 1 ascia ogni 200 secondi;
- 1 mina ogni 400 secondi;
- un bosco rimosso ogni 13 minuti e 20 secondi, se si spendono 4 asce;
- una montagna rimossa ogni 26 minuti e 40 secondi, se si spendono 4 mine.

Questi tempi sono volutamente lenti per la progressione normale. Più avanti potranno essere compensati con ricerche o upgrade dedicati, per esempio produzione strumenti +25%, +50% o aumento del limite massimo accumulabile.

## Regola generale

Quando una risorsa serve a sbloccare spazio, deve essere più lenta delle risorse economiche principali. Denaro, energia e ricerca alimentano il ciclo continuo; asce e mine devono invece creare decisioni di espansione.

## Upgrade iniziali

Gli upgrade di Step 10 sono volutamente semplici e non ripetibili (`maxLevel = 1`). Servono per introdurre la progressione senza creare ancora una curva economica complessa.

Valori iniziali:
- pala eolica produzione +50%;
- pala eolica durata +100%;
- centro ricerca piccolo +25%;
- batteria piccola +50%;
- ufficio piccolo +50%;
- generatore piccolo +50%;
- asce +25%;
- mine +25%.

Da ribilanciare dopo test manuale: costi denaro/ricerca e ordine percepito di acquisto.

## Step 12A - Durata pannelli solari

Il pannello solare passa da 30 secondi a 180 secondi di vita. La durata precedente era troppo breve rispetto al costo e alla necessità di affiancare un generatore. Il pannello deve introdurre la gestione del calore senza diventare una manutenzione troppo frequente.
## Step 16 - Primi edifici mid-game

Aggiunto il primo blocco di progressione industriale configurato nei JSON:

- Centrale a carbone: 155k$, 680 calore/s, vita 300s.
- Ufficio grande: 150k$, vende 200 energia/s e consuma energia operativa.
- Generatore medio: 100k$, converte 1k calore/s, raggio 1.
- Centrale a gas: 7.5M$, 25k calore/s, vita 300s.
- Centro ricerca grande: 10M$, produce 100 ricerca/s e consuma energia operativa.

Aggiunte ricerche collegate e primi upgrade specifici. La UI BUILD/RESEARCH/UPGRADE include i nuovi elementi. La formattazione numerica della UI ora usa prefissi SI: k, M, G, T, P, E, Z, Y.


## Upgrade multi-livello

Gli upgrade sono stati portati verso il modello idle/tycoon classico: ogni click aumenta il livello e il costo successivo cresce. Per ora la formula è semplice:

- effetto effettivo: `multiplier^level`
- costo prossimo livello: `baseCost * costGrowthMultiplier^currentLevel`

Questo rende gli upgrade acquistabili spesso all'inizio, ma progressivamente più costosi. I valori iniziali sono volutamente conservativi e restano modificabili in `Data/upgrades.json`.

## 2026-06-06 - Crescita costi upgrade più rapida

Gli upgrade multi-livello avevano moltiplicatori di costo ancora troppo permissivi, in genere tra 1.65 e 1.85. Sono stati aumentati indicativamente tra 2.15 e 2.60, con crescita più aggressiva sugli upgrade di durata e sugli edifici mid-game. Questo mantiene convenienti i primi livelli ma rende progressivamente più importante scegliere se investire in upgrade, nuovi edifici, ricerca o sblocco mappa.

## Step 20A - Base building roles and first balance pass

This pass starts the gameplay balancing milestone by assigning clearer economic roles to the current building set and reducing the strongest early-game distortion.

The starting loop is still immediate: the player starts with enough money to build the first wind turbine. The difference is that the first turbine now costs `$10` instead of `$1`, with starting money raised to `$10`. This keeps the tutorial-like first action intact, but prevents the base producer from feeling almost free forever.

Current intended roles:

| Building | Role | Balance intent |
| --- | --- | --- |
| Wind turbine | Starter direct energy | Cheap and simple, no heat, limited lifetime. Good for bootstrapping but not the final economy. |
| Battery | Starter storage | Prevents energy waste and supports selling/automation. |
| Small office | Starter automation | Converts stored production into automatic revenue, with small operating energy cost. |
| Small research center | Early research engine | Unlocks the first progression branch, but requires stable energy. |
| Solar panel | Early heat producer | Better output than wind, but only useful with heat conversion. |
| Small generator | Early heat converter | Pairs with solar panels and introduces heat planning. |
| Coal plant | Mid-game heat producer | Stronger thermal production with higher capital requirement. |
| Medium generator | Mid-game heat converter | Supports coal plants and the first industrial clusters. |
| Large office | Mid-game automation | Higher automatic selling throughput with relevant energy consumption. |
| Gas plant | Industrial heat producer | High-output target for planned generator networks. |
| Large research center | Industrial research engine | Expensive, high output, requires a stable mature grid. |

Main numeric changes:
- Starting money: `$1` -> `$10`.
- Starting max energy: `100` -> `200`.
- Wind turbine: cost `$1` -> `$10`, lifetime `60s` -> `75s`, storage support `5` -> `10`.
- Battery: cost `$50` -> `$200`.
- Small office: cost `$50` -> `$250`, auto sell `10/s` -> `8/s`, consumption `0.2/s` -> `0.25/s`.
- Small research center: cost `$1000` -> `$750`, output `1.25/s` -> `1/s`, consumption `0.5/s` -> `1/s`.
- Solar panel: cost `$200` -> `$400`, heat `10/s` -> `20/s`, lifetime `180s` -> `240s`.
- Small generator: cost `$1000` -> `$700`, conversion `20/s`, range `1`.
- Coal plant: cost `$155000` -> `$75000`, heat `680/s` -> `600/s`.
- Medium generator: cost `$100000` -> `$85000`, conversion `1000/s` -> `600/s`, range `1` -> `2`.
- Large office: cost `$150000` -> `$90000`, auto sell `200/s` -> `160/s`.
- Gas plant: cost `$7500000` -> `$2500000`, heat `25000/s` -> `6000/s`, lifetime `300s` -> `600s`.
- Large research center: cost `$10000000` -> `$2000000`, output `100/s` -> `50/s`, consumption `25/s` -> `20/s`.

Next balance checks:
- verify early payback for wind, solar+generator and research;
- tune research unlock costs against real research production;
- tune upgrade costs against the new building economy;
- verify heat explosion pressure with solar/coal/gas clusters.

## Step 20B - Research and upgrade balance alignment

This pass aligns research and upgrade costs with the revised Step 20A building economy.

The main goal is to avoid two problems:
- early upgrades that are either too cheap compared with the new `$10` first action;
- mid/industrial upgrades still priced for the previous multi-million economy.

Research cost intent:
- early unlocks remain reachable from small research;
- coal and large office enter mid game sooner;
- gas and large research are still industrial goals, but no longer require the old inflated economy;
- managers are positioned as useful automation unlocks before each building tier becomes obsolete.

Upgrade cost intent:
- first wind upgrades should be desirable but not immediate spam;
- solar/generator upgrades become the first meaningful heat economy optimization;
- coal/medium-generator/large-office upgrades now match the revised mid-game scale;
- gas and large-research upgrades remain expensive industrial investments, but are no longer priced far beyond the new building costs.

Main research changes:
- `coal_power`: `750` -> `600`.
- `office_large`: `1250` -> `900`.
- `generator_medium`: `2000` -> `1500`.
- `gas_power`: `10000` -> `6000`.
- `research_large`: `25000` -> `12000`.
- `wind_turbine_manager`: `300` -> `250`.
- `solar_panel_manager`: `1000` -> `750`.
- `coal_power_manager`: `15000` -> `6000`.
- `gas_power_manager`: `1000000` -> `50000`.

Main upgrade changes:
- early upgrade costs now start between `$80` and `$1500` instead of mixing very cheap and very expensive values;
- multipliers for production upgrades are mostly reduced from `1.5` to `1.35` to make repeated levels less explosive;
- lifetime upgrades are reduced from `2.0` to `1.5` where appropriate;
- mid-game upgrades now sit around `$90000-$120000`;
- industrial gas upgrades now sit around `$3000000-$4000000`, consistent with the `$2500000` gas plant.

## Step 20C - Heat, explosion and conversion balance

This pass makes the heat system less abrupt and easier to read while preserving heat as a real planning risk.

Core heat settings:
- Warning threshold: `60` -> `250`.
- Explosion threshold: `100` -> `500`.
- Energy conversion rate remains `1`.

Intended behavior:
- A solar panel without conversion now gives the player visible time to notice the problem instead of exploding almost immediately.
- A small generator handles one solar panel with margin.
- A coal plant is a mid-game thermal source that should be paired with at least one medium generator.
- A medium generator has range `2`, making thermal layouts easier to plan and less cramped.
- A gas plant remains an industrial risk and requires a generator network or upgraded conversion capacity.

Building heat/conversion changes:
- `generator_small`: heat conversion `20/s` -> `25/s`.
- `coal_power_plant`: heat `600/s` -> `500/s`.
- `generator_medium`: heat conversion `600/s` -> `750/s`, range `2`.
- `gas_power_plant`: heat `6000/s` -> `4500/s`.

Upgrade tuning:
- heat production and heat conversion repeated upgrade multipliers reduced to `1.30`.
- This keeps upgrades useful while reducing runaway thermal scaling.

UI readability:
- Added `HEAT RISK` to the properties panel.
- Thermal buildings now report `CONTROLLED`, `EXPLODES IN X S`, `RISK LOW`, `NO NEW HEAT`, or `EXPLOSION` depending on their current heat state.

## Step 20D - Expansion, tools and map unlock pacing

This pass aligns map expansion and terrain clearing with the revised Step 20 economy.

The old tool rates made terrain interaction too slow for the current pacing:
- `axesPerSecond` was `0.005`, meaning one axe every 200 seconds.
- `minesPerSecond` was `0.0025`, meaning one mine every 400 seconds.
- Clearing a forest cost `4` axes, so the first natural clear could take about 800 seconds.
- Clearing a mountain cost `4` mines, so the first natural clear could take about 1600 seconds.

New intent:
- Forest clearing should become an early expansion action after a short wait.
- Mountain clearing should remain slower, but not feel unreachable.
- Cloud unlock should be a meaningful mixed money/research investment, not a late-game wall.
- Tool storage is slightly larger so generated tools are less easily wasted.

Changes:
- `axesPerSecond`: `0.005` -> `0.025`.
- `minesPerSecond`: `0.0025` -> `0.0125`.
- `maxAxes`: `20` -> `25`.
- `maxMines`: `20` -> `25`.
- `forestClearAxesCost`: `4` -> `3`.
- `mountainClearMinesCost`: `4` -> `4` unchanged in absolute cost, but faster due to mine generation.
- `cloudUnlockMoneyCost`: `$2500` -> `$1500`.
- `cloudUnlockResearchCost`: `25` -> `40`.

Expected pacing:
- Forest clear from empty storage: about 120 seconds.
- Mountain clear from empty storage: about 320 seconds.
- Cloud unlock is cheaper in money but requires a more deliberate research investment.

## Step 20E - Early and mid-game progression pacing

This pass focuses on the first playable arc: wind bootstrap, first research, first thermal setup, and the transition into coal/mid-game.

Problem addressed:
- after the `$10` first wind turbine change, the first research center at `$750` still pushed the player into too much repeated manual wind selling;
- coal and medium generators were still too far away compared with the revised Step 20 economy;
- early automation and storage needed to become useful choices sooner.

New intended arc:
1. Build initial wind turbine.
2. Add several wind turbines and sell manually.
3. Choose a first support direction: battery, automation, or research.
4. Use research to unlock solar and small generator.
5. Use solar + generator as first efficient heat setup.
6. Move toward coal + medium generator as the first mid-game jump.

Building changes:
- `battery_small`: `$200` -> `$150`.
- `office_small`: `$250` -> `$180`, auto sell `8/s` -> `6/s`, consumption `0.25/s` -> `0.2/s`.
- `research_small`: `$750` -> `$350`, research `1/s` -> `0.75/s`, consumption `1/s` -> `0.75/s`.
- `solar_panel`: `$400` -> `$300`, heat `20/s` -> `18/s`.
- `generator_small`: `$700` -> `$450`.
- `coal_power_plant`: `$75000` -> `$35000`, heat `500/s` -> `450/s`.
- `generator_medium`: `$85000` -> `$40000`, conversion `750/s` -> `650/s`.
- `office_large`: `$90000` -> `$45000`, auto sell `160/s` -> `120/s`, consumption `4/s` -> `3/s`.
- `gas_power_plant`: `$2500000` -> `$1500000`, heat `4500/s` -> `3500/s`.
- `research_large`: `$2000000` -> `$1000000`, research `50/s` -> `35/s`, consumption `20/s` -> `15/s`.

Research changes:
- `coal_power`: `600` -> `450`.
- `office_large`: `900` -> `650`.
- `generator_medium`: `1500` -> `1000`.
- `gas_power`: `6000` -> `4500`.
- `research_large`: `12000` -> `9000`.
- `coal_power_manager`: `6000` -> `4000`.

Upgrade alignment:
- first early upgrade prices were reduced to match cheaper early/mid buildings;
- mid-game coal/office/generator upgrades were reduced to match the new `$35000-$45000` tier;
- industrial upgrades were reduced to match the revised gas/research-large tier.

## Step 21A - Clear operational issue text

This pass starts the gameplay feedback milestone. It does not change simulation rules or balance numbers.

The properties panel now includes an `ISSUE` row for selected buildings. The row explains the practical reason behind the current operational state instead of showing only a generic state label.

Examples:
- `NEEDS STORED ENERGY` for buildings blocked by missing energy.
- `PLACE GENERATOR IN RANGE` for heat producers without conversion.
- `HEAT ABOVE WARNING` for overheated buildings.
- `REPLACE OR MANAGE` for expired buildings.
- `RESTORE OR DEMOLISH` for exploded buildings.
- `HEAT CONVERSION OK`, `PRODUCING ENERGY`, `PRODUCING RESEARCH`, `SELLING ENERGY`, `ABSORBING HEAT`, or `STORING ENERGY` for active buildings.

Intent:
- make failures readable without opening code or guessing mechanics;
- make heat/conversion states clearer after Step 20C;
- keep the UI consistent with the existing properties table.

## Step 21B - Map operational state badges

This pass improves readability without changing balance.

Buildings now show compact map badges for important operational states:
- `E` means the building needs stored energy.
- `G` means a heat producer needs a generator/converter in range.
- `H` means heat is above the warning threshold.
- `T` means the building has expired.
- `X` means the building has exploded.

The badges are intentionally small and sit on the building tile, so the map communicates problems before the player opens the properties panel.

## Step 21C - Status bar badge legend

This pass improves the readability of the map operational badges.

The status bar now shows a compact legend when enough horizontal space is available:
- `E ENERGY`
- `G GEN`
- `H HEAT`
- `T TIME`
- `X BOOM`

The legend is intentionally responsive and disappears on narrower windows to avoid crowding the main status message.

## Step 21D - Clear failure messages

This pass improves feedback when the player tries an action that cannot be completed.

The status bar now translates common failure reasons into short actionable messages:
- build failures explain money, research, terrain, occupied cell or space problems;
- research failures explain missing points, prerequisites or already completed research;
- terrain clear failures explain missing axes/mines or blocked cells;
- cloud unlock failures explain missing money/research or invalid cloud selection;
- upgrade failures explain missing money/research, missing research requirement or max level.

This is a UI/readability change only.

## Step 21E - Status bar selected building feedback

This pass improves the status bar as the main communication surface.

When a building is selected, the status bar now reports the selected building state unless a higher-priority action message is active. This helps the player understand the current selection without constantly reading the full properties panel.

Priority remains:
1. demolish confirmation;
2. save/load message;
3. latest action result/failure;
4. selected building operational summary;
5. default build/select prompt.

The status bar is also included in UI hit-testing, preventing map clicks through the bottom UI.

## Step 21F - Feedback system documentation

This pass consolidates Milestone 21.

A new `docs/FEEDBACK_SYSTEM.md` file records how the game communicates operational state to the player: map badges, properties panel `ISSUE`, heat risk, status bar priority and failure message style.

This is a documentation-only step.

## Step 22A - Manager visibility in properties

This pass starts the automation/manager milestone.

The properties panel now includes a `MANAGER` row. It explains whether the selected building has a related manager research and whether that manager is already active.

Examples:
- `-` means no manager exists for this building.
- `UNLOCK: Gestore pale eoliche` means the building can be managed after the related research.
- `ACTIVE: Gestore pale eoliche` means the manager already covers this building.

This is a readability/UI step only.

## Step 22B - Manager renewal feedback

This pass improves automation feedback.

Manager renewals already existed in the simulation. The UI now makes them visible:
- successful renewals show how many buildings were renewed and how much money was spent;
- failed renewals show how many managed expired buildings could not be renewed because money was missing;
- identical repeated manager failures are not allowed to continuously overwrite newer feedback.

This is a feedback step only.

## Step 22C - Manager map badge

Managed buildings now show a compact `M` badge directly on the map. This makes automation coverage visible without selecting each building.

The `M` badge is a readability-only marker and does not affect manager behavior.

## Step 22D - Manager research impact text

Manager research cards now show their current impact. Instead of only saying that they provide automatic management, they report how many existing buildings are affected and how many covered buildings are expired.

Examples:
- `WILL MANAGE 0 BUILT`
- `WILL MANAGE 3 BUILT`
- `MANAGING 5 BUILT`
- `MANAGING 5 BUILT | EXPIRED 2`

This is a UI/readability step only.

## Step 23A - Cloud unlock map preview

This pass begins map expansion readability.

Selecting a cloud tile now highlights the exact cloud tiles that would be revealed by the unlock action. The preview uses the same `AreaUnlockSystem.GetUnlockableCloudTiles` method used by the real unlock logic.

The preview is blue when the unlock is currently possible and red when the selected cloud cannot currently be unlocked, usually because money or research is missing.

This is a UI/readability step only.

## Step 23B - Terrain clear map preview

Selected obstacles now show a direct map preview for clearing.

Forests display an `A#` marker for required axes. Mountains display an `M#` marker for required mines. The preview is green when the player can clear the tile and red when the required tools are missing.

This is a readability/UI step only.

## Step 23C - Expansion and obstacle property clarity

Selected forests, mountains and cloud tiles now communicate expansion costs more directly.

Forests and mountains show available tools against required tools, for example `2 / 3 AXES` or `1 / 4 MINES`. Cloud unlocks show current money and research against the required values, and the `ISSUE` row explains whether money, research or revealable tiles are missing.

This is a readability/UI step only.

## Step 23D - Area unlock result summary

Successful cloud unlocks now show a compact terrain summary in the status bar.

Example:
`AREA UNLOCKED 5: FOREST 1, LAND 3, MOUNTAIN 1`

The message is generated from the existing unlock result and does not change expansion behavior.

## Step 23E - Expansion system documentation

This pass consolidates Milestone 23.

A new `docs/EXPANSION_SYSTEM.md` file records the current expansion loop: cloud unlock preview, obstacle clear preview, properties panel text, unlock result summaries and the source-of-truth systems.

This is a documentation-only step.

## Step 23F - Building range overlay

Selecting a built heat converter now highlights its operational range directly on the map. Heat producers inside the selected converter range receive a stronger outline.

This improves readability only. Heat conversion behavior, range values and balance are unchanged.

## Step 23G - Heat coverage inverse feedback

Selecting a heat producer now highlights the active heat converters that cover it. This complements the Step 23F range overlay, which starts from the converter side.

This improves map readability only. Heat conversion behavior, ranges and balance are unchanged.

## Step 23H - Heat converter placement range preview

When a heat converter building tool is selected, the map now previews the future operational range around the hovered tile. Valid placement uses the normal range overlay; invalid placement is shown in red/attenuated form.

This is a UI/readability step only. Build validation, heat conversion behavior and balance are unchanged.

## Step 23I - Milestone 23 final documentation

This pass closes Milestone 23.

The milestone now covers map expansion readability, obstacle clearing readability and heat range readability. All changes in this final documentation pass are descriptive only.

No balance values were changed.

## Step 23J - Useful stability tests

Added useful core tests to protect the rules that Milestone 23 visualizes.

The tests do not change balance. They verify heat range behavior, cloud preview behavior and terrain clear validation.

## Step 24A - First objective hint

Added a non-blocking objective hint for early onboarding.

No balance values were changed. The objective sequence follows the current economy: first build wind, sell energy, then reach office/research/battery/solar/generator progression.

## Step 24B - Early game checklist

Added a compact early checklist for onboarding.

No balance values were changed. Checklist completion is derived from existing world state.

## Step 24C - Contextual heat and generator hints

Added clearer heat/generator guidance in build cards and property rows.

No balance values were changed.

## Step 24D - Help panel / quick guide

Added a non-modal help panel for onboarding.

No balance values were changed.

## Step 24E - Help panel current objective/checklist

Improved HELP panel with current objective and checklist status.

No balance values were changed.

## Step 24F - Milestone 24 final documentation

This pass closes Milestone 24.

The milestone adds onboarding and guidance only. No balance values were changed.

## Step 25A - Mid-game objective hints

Added mid-game objective hints for upgrades, research, expansion, managers and next heat tiers.

No balance values were changed.

## Step 25B - Goal-aware HELP details

Added HELP details for missing money, research, tools, cloud unlock costs and upgrade readiness.

No balance values were changed.

## Step 25C - Progression bottleneck feedback

Added bottleneck text to HELP for energy, money, research, heat, expansion, tools, storage and available upgrades/research.

No balance values were changed.

## Step 25D - Progression advisor extraction and tests

Moved progression guidance into a Core advisor and added tests.

No balance values were changed.

## Step 25E - Milestone 25 final documentation

Closed Milestone 25 documentation.

No balance values were changed. The next milestone will add new buildings and will require careful balance notes per building.

## Step 26A - Substation / Transformer

Added Substation / Transformer balance values.

Initial values:
- building id: `substation_small`;
- cost: $2,200;
- required research: `grid_substation`;
- research cost: R180;
- prerequisite research: `generator_small`;
- energy consumption: 0.5/s;
- energy efficiency bonus: +10%;
- size: 1 x 1;
- lifetime: none.

Balance intent: mid-game support building that improves global grid output after the player has established heat conversion.

## Step 26B - Heat sink / Raffreddatore

Added Heat sink / Raffreddatore balance values.

Initial values:
- building id: `heat_sink_small`;
- cost: $900;
- required research: `heat_management`;
- research cost: R120;
- prerequisite research: `generator_small`;
- energy consumption: 0.4/s;
- heat dissipation: 18/s;
- range: 1;
- size: 1 x 1;
- lifetime: none.

Balance intent: defensive alternative to generators. It reduces heat risk but does not produce energy.

## Step 26C - Maintenance center / Centro manutenzione

Added Maintenance center balance values.

Initial values:
- building id: `maintenance_center_small`;
- cost: $3,200;
- required research: `maintenance_center`;
- research cost: R300;
- prerequisite research: `grid_substation`;
- energy consumption: 0.8/s;
- maintenance efficiency bonus: 25% slower lifetime wear;
- minimum lifetime decay multiplier is capped at 25%;
- size: 1 x 1;
- lifetime: none.

Balance intent: reduce operational churn before complete manager automation.

## Step 26D - Tool warehouse / Magazzino strumenti

Added Tool warehouse balance values.

Initial values:
- building id: `tool_warehouse_small`;
- cost: $1,800;
- required research: `tool_storage`;
- research cost: R220;
- prerequisite research: `heat_management`;
- energy consumption: 0.25/s;
- tool capacity bonus: +25 axes and +25 mines;
- size: 1 x 1;
- lifetime: none.

Balance intent: improve expansion logistics without accelerating tool generation.

## Step 26E - Geothermal plant / Centrale geotermica

Added Geothermal plant balance values.

Initial values:
- building id: `geothermal_plant`;
- cost: $12,000;
- required research: `geothermal_power`;
- research cost: R1,600;
- prerequisite research: `coal_power`;
- size: 2 x 2;
- heat production: 180/s;
- energy consumption: 1.5/s;
- lifetime: 600 seconds.

Balance intent: stable mid-game heat source that requires conversion capacity and grid support.

## Step 26F - Data center

Added Data center balance values.

Initial values:
- building id: `data_center`;
- cost: $250,000;
- required research: `data_center`;
- research cost: R8,000;
- prerequisite research: `research_large`;
- category: Corporation;
- size: 2 x 2;
- research output: 120/s;
- energy consumption: 80/s;
- lifetime: none.

Balance intent: late-game energy sink that converts surplus energy into research velocity.

## Step 26G - Nuclear reactor

Added Nuclear reactor balance values.

Initial values:
- building id: `nuclear_reactor`;
- cost: $18,000,000;
- required research: `nuclear_power`;
- research cost: R25,000;
- prerequisite research: `geothermal_power`, `maintenance_center`, `data_center`;
- category: HeatProducer;
- size: 3 x 3;
- heat production: 9,000/s;
- energy consumption: 120/s;
- lifetime: 900 seconds.

Added upgrades:
- `nuclear_heat_1`: heat-production multiplier, cost $22,000,000 and R5,000, growth 2.45;
- `nuclear_lifetime_1`: lifetime multiplier, cost $28,000,000 and R6,500, growth 2.45.

Balance intent: end-game risky heat source. It should require a mature conversion network and enough operational stability to avoid becoming a simple automatic win condition.

## Milestone 26 pause balance summary

Current balance checkpoint after Step 26G:

| Building | Cost | Research cost | Main effect |
|---|---:|---:|---|
| `substation_small` | $2,200 | R180 | +10% grid energy efficiency |
| `heat_sink_small` | $900 | R120 | 18/s heat dissipation, range 1 |
| `maintenance_center_small` | $3,200 | R300 | 25% slower lifetime wear |
| `tool_warehouse_small` | $1,800 | R220 | +25 axes and +25 mines capacity |
| `geothermal_plant` | $12,000 | R1,600 | 180/s heat, 1.5/s energy input |
| `data_center` | $250,000 | R8,000 | 120/s research, 80/s energy input |
| `nuclear_reactor` | $18,000,000 | R25,000 | 9,000/s heat, 120/s energy input |

These values are provisional. The next pass should check the full curve from coal to gas to nuclear, rather than rebalancing the reactor in isolation.

Next balance work:
- verify progression from coal to geothermal;
- verify that Data center energy demand is reachable but meaningful;
- verify that heat sink and generator capacity make sense with geothermal and nuclear;
- verify that maintenance and managers do not overlap too much;
- verify that tool warehouse helps expansion without trivializing obstacles;
- verify that nuclear heat output is powerful but still requires planned conversion capacity.

## Step 26H - Milestone 26 final balance closure

Milestone 26 is now closed as a content block. The definitive runtime balance reference for this milestone is `docs/MILESTONE_26_BALANCE.md`.

The important final decisions are:

- the new buildings are role-based rather than linear upgrades;
- the Substation, Heat sink, Maintenance center and Tool warehouse add support mechanics;
- Geothermal, Data center and Nuclear reactor reuse existing mechanics so the game does not fragment into too many special systems;
- the Nuclear reactor remains a heat producer, not direct electricity;
- the next milestone should focus on save compatibility and stability.

Current tool values are now faster than the earliest prototype balance: axes generate at `0.025/s`, mines at `0.0125/s`, base caps are `25/25`, clearing a forest costs `3` axes and clearing a mountain costs `4` mines. The Tool warehouse does not increase generation speed; it increases storage capacity by `+25` for both axes and mines.

Current heat-chain reference:

| Producer | Heat/s | Balance meaning |
|---|---:|---|
| `solar_panel` | 18 | early heat introduction, one small generator covers it |
| `coal_power_plant` | 450 | mid-game heat source, medium generator target |
| `geothermal_plant` | 180 | stable heat source with lower pressure and larger footprint |
| `gas_power_plant` | 3500 | industrial heat source requiring clustered conversion |
| `nuclear_reactor` | 9000 | end-game heat source requiring mature support infrastructure |

Do not add another production tier immediately after this. The project now needs a persistence/stability milestone so old saves, new building properties, UI ids and data-version handling remain reliable.

