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



## Reactor reference: upgrade a livelli e gestori

I nuovi dati di riferimento mostrano che gli upgrade devono essere pensati come acquisti ripetibili a livelli: livello 0 iniziale, livello 1 dopo il primo acquisto, poi livelli successivi con costo crescente. Questo è più adatto a un idle/tycoon rispetto agli upgrade monouso.

Direzione consigliata per GridPowerTycoon:
- mantenere gli upgrade attuali funzionanti;
- convertire `upgrades.json` verso un modello multi-livello con `baseCostMoney`, `costGrowthMultiplier` ed `effectPerLevel`;
- mostrare nella UI il livello corrente, il prossimo costo e l'effetto del prossimo livello;
- introdurre in seguito le ricerche gestore per rinnovare automaticamente edifici scaduti.
