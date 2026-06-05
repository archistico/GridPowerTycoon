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
