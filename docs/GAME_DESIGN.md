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
