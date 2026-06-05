# Game Design

GridPower Tycoon è un gestionale energetico su mappa a griglia. Il giocatore costruisce edifici su isole, produce energia, vende energia per ottenere denaro, ricerca tecnologie, automatizza la vendita e si espande su nuove aree.

La prima versione si concentra su una mappa 2D top-down con celle di tipo acqua, terra, bosco, montagna e nuvola. Solo le celle di terra libere sono edificabili.

Le risorse iniziali sono energia, energia massima, ricerca, denaro, asce e mine.

I valori di bilanciamento stanno in JSON, in modo che costi, produzione, durata, capacità e moltiplicatori possano essere modificati senza ricompilare.

## Mappa e arcipelago
La mappa predefinita usa un formato JSON a due layer. `rows` descrive ciò che il giocatore vede, mentre `hiddenRows` descrive il terreno reale sotto le nuvole. Questo permette di disegnare isole già presenti nella mappa ma nascoste inizialmente, rivelando progressivamente terreno libero, boschi e montagne senza cambiare codice. La mappa attuale è 64x40 e rappresenta un arcipelago con un'isola iniziale visibile e diverse isole coperte da nuvole.
