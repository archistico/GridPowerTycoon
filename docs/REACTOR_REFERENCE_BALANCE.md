# Reactor reference balance

Questo documento conserva i valori indicati come riferimento per valutare nomi, ordini di grandezza e progressione. Non è una specifica da copiare automaticamente nel gameplay di GridPowerTycoon.

Nel progetto usiamo la notazione SI invece di `B` come abbreviazione generica:

| Simbolo | Nome | Fattore |
|---|---:|---:|
| k | kilo | 10^3 |
| M | mega | 10^6 |
| G | giga | 10^9 |
| T | tera | 10^12 |
| P | peta | 10^15 |
| E | exa | 10^18 |
| Z | zetta | 10^21 |
| Y | yotta | 10^24 |

## Riferimenti utili per la curva

La progressione Reactor alterna quattro famiglie di edificio: produttori diretti di energia, produttori di calore, generatori/converter e strutture di vendita automatica/ricerca. Per GridPowerTycoon conviene mantenere questo ritmo, ma adattando costi, consumi e durata alla nostra mappa a isole e al sistema energia operativa.

Primo blocco mid-game introdotto in GridPowerTycoon:

| Edificio | Costo | Effetto |
|---|---:|---|
| Centrale a carbone | 155k$ | 680 calore/s, vita 300s |
| Ufficio grande | 150k$ | vende 200 energia/s, consuma energia operativa |
| Generatore medio | 100k$ | converte 1k calore/s in energia, raggio 1 |
| Centrale a gas | 7.5M$ | 25k calore/s, vita 300s |
| Centro ricerca grande | 10M$ | 100 ricerca/s, consuma energia operativa |

Questi valori sono reference-inspired: mantengono gli ordini di grandezza della fonte, ma la ricerca, i consumi e gli sblocchi sono adattati al nostro gioco.


## Riferimento Reactor: upgrade ripetibili

Questi valori sono stati forniti come riferimento per il comportamento degli aggiornamenti in Reactor. La caratteristica importante non è solo il singolo valore, ma il fatto che l'upgrade parte da livello 0, passa a livello 1 dopo l'acquisto, poi può essere acquistato ancora a livello 2, 3 e così via con costo crescente.

Per GridPowerTycoon questo suggerisce di evolvere l'attuale sistema upgrade da bonus singolo a **upgrade multi-livello**. Ogni upgrade dovrebbe avere almeno: `baseCostMoney`, eventuale `baseCostResearch`, `level`, `maxLevel` opzionale, `costGrowthMultiplier`, `effectPerLevel`, `effectType`, `targetBuildingId` o `targetGlobalStat`.

| Upgrade Reactor | Costo iniziale | Livello iniziale | Effetto |
|---|---:|---:|---|
| Pala eolica - produzione | 250$ | 0 | produzione energia +50% per livello |
| Pala eolica - tempo di vita | 15$ | 0 | periodo di vita +100% per livello |
| Centro di ricerca grande | 25k$ | 0 | produzione ricerca +25% per livello |
| Ufficio | 1k$ | 0 | vendita energia +50% per livello |
| Batteria | 300$ | 0 | energia massima +50% per livello |
| Generatore - calore massimo | 1k$ | 0 | calore massimo +100% per livello |
| Generatore - efficacia | 400$ | 0 | conversione calore +25% per livello |
| Pannello solare - produzione | 1k$ | 0 | produzione calore +50% per livello |
| Pannello solare - tempo di vita | 10k$ | 0 | periodo di vita +100% per livello |
| Centrale a carbone - produzione | 125k$ | 0 | produzione calore +50% per livello |
| Centrale a carbone - tempo di vita | 5M$ | 0 | periodo di vita +100% per livello |
| Centrale a gas - produzione | 20M$ | 0 | produzione calore +50% per livello |
| Centrale a gas - tempo di vita | 125M$ | 0 | periodo di vita +100% per livello |

Nota di design: per GridPowerTycoon non conviene applicare tutti questi valori alla lettera. Alcuni valori possono essere usati come curva di riferimento, ma il nostro gioco ha consumo energetico operativo, mappa, nuvole, strumenti, salvataggio offline e isole, quindi i costi andranno adattati.

### Proposta tecnica per upgrade multi-livello

Formato JSON futuro possibile:

```json
{
  "id": "wind_turbine_energy",
  "name": "Pale eoliche migliorate",
  "description": "Aumenta la produzione energia delle pale eoliche.",
  "targetBuildingId": "wind_turbine",
  "effectType": "MultiplyEnergyProduction",
  "effectPerLevel": 1.5,
  "baseCostMoney": 250,
  "baseCostResearch": 0,
  "costGrowthMultiplier": 2.5,
  "maxLevel": 0,
  "requiredResearchId": null
}
```

Regola proposta:

```text
costo livello successivo = baseCost * costGrowthMultiplier ^ livelloCorrente
moltiplicatore totale = effectPerLevel ^ livelloCorrente
maxLevel = 0 significa nessun limite pratico
```

In alternativa, per alcuni upgrade può essere più gestibile una crescita additiva:

```text
bonus totale = 1 + livelloCorrente * percentualePerLivello
```

La scelta va fatta per `effectType`. Per produzioni e vendite l'esponenziale è più tipico da idle/tycoon; per riduzioni di costo, sicurezza e soglie calore è più prudente usare limiti o crescita additiva.

## Riferimento Reactor: ricerche e gestori

Anche le ricerche in Reactor indicano una direzione importante: non sbloccano solo edifici, ma anche **gestori** che rinnovano automaticamente gli edifici scaduti.

| Ricerca Reactor | Costo ricerca | Effetto |
|---|---:|---|
| Centro di ricerca piccolo | 100 | abilita produzione punti ricerca |
| Pala eolica - gestore | 300 | rinnova automaticamente le pale eoliche |
| Ufficio | 500 | abilita vendita automatica energia |
| Pannello solare | 2.5k | apre tecnologia solare |
| Batteria | 250 | abilita batterie |
| Pannello solare - gestore | 1k | rinnova automaticamente pannelli solari |
| Centrale a carbone | 50k | apre tecnologia a carbone |
| Centrale a carbone - gestore | 15k | rinnova automaticamente centrali a carbone |
| Ufficio grande | 50k energia nel riferimento originale | vende più energia automaticamente |

Per GridPowerTycoon i gestori sono un ottimo prossimo sistema dopo gli upgrade multi-livello. La logica potrebbe essere:

```text
ManagerRenewalSystem:
- controlla edifici Expired;
- se esiste la ricerca manager per quel buildingId;
- se ci sono soldi sufficienti;
- rimpiazza automaticamente l'edificio;
- eventualmente applica un costo maggiorato o una priorità configurabile.
```

Formato dati futuro possibile:

```json
{
  "id": "manager_wind_turbine",
  "name": "Gestore pale eoliche",
  "cost": 300,
  "unlockManagerForBuildingIds": [ "wind_turbine" ],
  "requiredResearchIds": []
}
```

Decisione provvisoria: prima introdurre upgrade multi-livello, poi introdurre gestori automatici. In questo modo le due automazioni restano separate e testabili.
