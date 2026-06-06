# Game Design

GridPower Tycoon è un gestionale energetico su mappa a griglia. Il giocatore costruisce edifici su isole, produce energia, vende energia per ottenere denaro, ricerca tecnologie, automatizza la vendita e si espande su nuove aree.

La prima versione si concentra su una mappa 2D top-down con celle di tipo acqua, terra, bosco, montagna e nuvola. Solo le celle di terra libere sono edificabili.

Le risorse iniziali sono energia, energia massima, ricerca, denaro, asce e mine.

I valori di bilanciamento stanno in JSON, in modo che costi, produzione, durata, capacità e moltiplicatori possano essere modificati senza ricompilare.

## Mappa e arcipelago
La mappa predefinita usa un formato JSON a due layer. `rows` descrive ciò che il giocatore vede, mentre `hiddenRows` descrive il terreno reale sotto le nuvole. Questo permette di disegnare isole già presenti nella mappa ma nascoste inizialmente, rivelando progressivamente terreno libero, boschi e montagne senza cambiare codice. La mappa attuale è 64x40 e rappresenta un arcipelago con un'isola iniziale visibile e diverse isole coperte da nuvole.

## Mappa grande e scoperta progressiva
La mappa principale è ora pensata come un arcipelago ampio, con isola iniziale al centro e isole circostanti coperte da nuvole. La dimensione 128x80 evita che l'intera area sia leggibile in un solo colpo d'occhio. Lo zoom minimo è volutamente limitato: il giocatore deve usare il pan per scoprire e pianificare l'espansione.

## Offline progress rule
Offline progress uses the same economy direction as live play, but with a safety rule: buildings may expire while the player is away, while heat explosions are not triggered offline. This prevents a return-to-game experience where a large part of the grid is destroyed without player interaction. The maximum offline duration is controlled by `economy.json` through `maxOfflineSeconds`.

## Cloud expansion groups

On the large map, unlocking clouds one cell at a time is too slow and repetitive. Cloud expansion now reveals a small connected group around the selected cell. The amount is controlled by JSON, so the pacing can be adjusted without recompiling. The current default reveals up to 9 connected cloud tiles within radius 2 for a fixed money/research cost.
## Step 16 - Primi edifici mid-game

Aggiunto il primo blocco di progressione industriale configurato nei JSON:

- Centrale a carbone: 155k$, 680 calore/s, vita 300s.
- Ufficio grande: 150k$, vende 200 energia/s e consuma energia operativa.
- Generatore medio: 100k$, converte 1k calore/s, raggio 1.
- Centrale a gas: 7.5M$, 25k calore/s, vita 300s.
- Centro ricerca grande: 10M$, produce 100 ricerca/s e consuma energia operativa.

Aggiunte ricerche collegate e primi upgrade specifici. La UI BUILD/RESEARCH/UPGRADE include i nuovi elementi. La formattazione numerica della UI ora usa prefissi SI: k, M, G, T, P, E, Z, Y.


## Gestori automatici

I gestori automatici sono sblocchi di ricerca che riducono la manutenzione manuale. Quando un edificio con vita limitata scade, il gestore può rinnovarlo automaticamente pagando il costo dell'edificio, purché il giocatore abbia denaro sufficiente.

I gestori rinnovano solo edifici scaduti. Gli edifici esplosi restano distrutti e devono essere gestiti in modo diverso in una futura fase di gameplay.

## Milestone 24 onboarding direction

The first onboarding pass uses non-blocking status bar objectives instead of modal tutorial screens.

The objective hint is intentionally lightweight. It is derived from current world state and does not create persistent mission state. This keeps saves stable and avoids forcing the player through a rigid tutorial.

Initial objective sequence:
1. build a wind turbine;
2. sell stored energy and build a small office;
3. build a small research center;
4. build a small battery;
5. build a solar panel;
6. place a generator in range of heat sources;
7. expand the map and upgrade buildings.

Action results and selected-building feedback remain higher priority than tutorial text.

### Early checklist

The early onboarding now combines a single current objective with a compact checklist.

The checklist is not a quest system. It is a read-only UI summary derived from world state. It helps the player understand the first progression arc without adding persistent tutorial state.

Current checklist:
- build wind turbine;
- build small office;
- build research center;
- build small battery;
- build solar + generator.

The checklist disappears once all items are complete.

### Contextual heat guidance

Heat onboarding now appears directly in the places where the player is already looking.

Heat producers explain that a generator must be placed in range. Heat converters explain that they should be placed near a heat source and that the range preview should be used before building.

This keeps onboarding non-blocking and avoids modal tutorial screens.

### Quick guide panel

A non-modal quick guide can now be opened with the HELP button or the H key.

The guide summarizes the core early game loop: produce energy, sell energy, build office/research/battery, use generators to convert heat, clear obstacles and unlock clouds. This keeps onboarding accessible without interrupting the player.

### Help panel current progress

The HELP panel now includes a current-progress section. It repeats the active objective and early checklist, making the guide useful both as documentation and as a status reference during play.

The section remains derived from world state and does not introduce quest save data.

## Milestone 24 final onboarding state

Milestone 24 introduces non-blocking onboarding. The player is guided without modal tutorials, forced steps or saved quest state.

The final onboarding set is:

- a dynamic objective in the status bar;
- an early-game checklist shown in the map area;
- contextual heat/generator hints in build cards and property rows;
- a non-modal HELP panel opened with the `HELP` button or the `H` key;
- a dynamic `CURRENT` section inside HELP showing the current objective and checklist state;
- a static `BASICS` section inside HELP explaining the main game loops and controls.

The onboarding state is always derived from the current world:
- built buildings;
- available money;
- heat producers with or without generator coverage;
- existing world/resource state.

No persistent mission state is stored. This keeps save-data stable and avoids rigid tutorial flow.

Final early progression guidance:
1. build a wind turbine;
2. sell energy and build a small office;
3. build a small research center;
4. build a small battery;
5. build a solar panel;
6. place a generator in range of heat sources;
7. expand the map, upgrade buildings and keep the grid stable.

## Milestone 25 progression guidance

Milestone 25 starts by extending guidance beyond the first onboarding loop.

After the early sequence is complete, the objective system no longer falls back immediately to a generic message. It now derives mid-game objectives from the current world and points the player toward the next useful progression step.

Current mid-game objective priorities:
1. buy the first relevant upgrade;
2. complete available research;
3. produce research for the next available research item;
4. unlock clouds when expansion is still available;
5. clear forests or mountains when blocked terrain exists;
6. unlock a manager;
7. build the next heat power tier;
8. buy later upgrades.

This remains a guidance layer only. It does not create persistent quest state.

### Goal-aware HELP details

The HELP panel now explains not only the current objective, but also the immediate next action.

Examples:
- if money is missing, the guide shows the missing amount;
- if research is missing, the guide shows the research gap;
- if a cloud can be unlocked, the guide tells the player to select a cloud and click unlock;
- if terrain can be cleared, the guide tells the player to select the obstacle and click clear;
- if an upgrade is available, the guide points the player to the Upgrade tab.

This remains world-state derived guidance and does not introduce persistent quest data.

### Progression bottleneck feedback

The HELP panel now classifies the current bottleneck. This is separate from the objective and the next action.

The bottleneck helps the player understand the systemic reason for slow progress: energy, money, research, heat coverage, expansion resources, storage or available upgrades/research.

### Progression advisor

Progression guidance is now centralized in Core through `ProgressionAdvisor`.

The advisor produces three UI-facing strings: current objective, next practical detail and bottleneck. This keeps the behavior testable and avoids spreading progression rules across rendering code.
