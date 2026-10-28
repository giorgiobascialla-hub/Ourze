# HDLSS5 1.5.1

## 01 · Orientarsi nell’interfaccia

HDLSS5 organizza la configurazione per gioco. Inizia dalla libreria, seleziona un titolo e poi apri la scheda del componente che vuoi utilizzare.

### La schermata principale

La libreria mostra i giochi rilevati o aggiunti. La copertina e il nome identificano il gioco selezionato: tutte le operazioni sui componenti riguardano quel gioco. Cerca giochi filtra la libreria. Apri cartella apre la posizione del gioco; Rimuovi dalla libreria nasconde la voce e non disinstalla il gioco.

### Le tre schede

DLSS 5 · OptiScaler contiene OptiScaler Standard e F5 · Janblade. DLSS 5 · ReShade contiene il percorso guidato ReShade e Neural Rendering. DLL e Streamline gestisce le librerie e i modelli. I menu opzionali separano effetti aggiuntivi e loader avanzati dai passaggi principali.

### Impostazioni generali

Il menu delle impostazioni raccoglie i comandi generali dell’app, come lingua, tema e gestione della libreria. Guida spiega le procedure; Backup riguarda il recupero. Dettagli apre spiegazioni e opzioni avanzate del singolo componente. Esempio: per aggiornare una DLL del gioco selezionato apri DLL e Streamline, non le impostazioni generali.

## 02 · Colori, pulsanti e stato

Leggi sempre lo stato del componente prima di premere un pulsante. Il colore è un aiuto e non sostituisce il messaggio.

### Significato dei colori

Magenta: il controllo ha individuato un aggiornamento disponibile. Blu: azione normale, installazione o controllo. Giallo: installazione o scelta evidenziata, secondo il comando. Rosso: errore, incompatibilità o verifica non riuscita. Un pulsante scuro e attenuato è disabilitato: rimane visibile per indicare l’azione, ma non è utilizzabile nello stato attuale.

### Versioni e avanzamento

Versione installata descrive i file rilevati; versione disponibile indica quella proposta dal controllo o dal catalogo. Catalogo locale/incluso non significa verifica online appena eseguita. Durante un’operazione il pulsante può mostrare avanzamento e percentuale: attendi il risultato, non soltanto il 100%.

### Esempio pratico

Se leggi Aggiornato e Aggiorna è spento, non serve reinstallare. Se leggi Verifica non riuscita, premi Controlla aggiornamenti quando la connessione è disponibile. File installati non certifica che l’effetto sia attivo nel gioco: controllalo in una scena giocabile.

## 03 · Libreria ed eseguibile

Aggiungi o seleziona il gioco prima di configurare qualsiasi componente. Chiudi il gioco prima di installare, aggiornare o rimuovere file.

### Rilevare e aggiungere giochi

Usa Scansiona per cercare installazioni nelle librerie supportate. Aggiungi gioco permette di indicare un’installazione non rilevata. Se un gioco è stato nascosto, usa Ripristina giochi nascosti. Cambiare copertina modifica la presentazione nella libreria, non i file del gioco.

### Scelta automatica dell’eseguibile

HDLSS5 cerca l’eseguibile di rendering e la destinazione appropriata. Se non riesce a stabilirli con sufficiente certezza, mostra un blocco o richiede una verifica: non significa che qualsiasi .exe sia adatto. Eseguibile e grafica serve alle correzioni avanzate; cambiare l’API dichiarata non converte il motore del gioco.

### Launcher e Game Pass

Un launcher può avviare un processo diverso da quello che renderizza il gioco. I file protetti o le cartelle non scrivibili possono impedire l’installazione. Esempio: se vengono trovati launcher.exe e un eseguibile nella cartella Binaries, controlla il percorso proposto dall’app prima di correggerlo manualmente. Non rimuovere protezioni del sistema per forzare la copia.

## 04 · Scegliere il percorso

OptiScaler e ReShade sono percorsi distinti. Scegli in base alla funzione desiderata e alla compatibilità del gioco, non installando tutto insieme.

### Percorso OptiScaler

Apri DLSS 5 · OptiScaler. Standard e F5 · Janblade hanno capacità diverse. Standard non acquisisce automaticamente le funzioni NR del fork F5. Le opzioni disponibili dipendono dalla build selezionata; apri Dettagli per quelle avanzate.

### Percorso ReShade

Apri DLSS 5 · ReShade. Segui 1. Installa ReShade e poi 2. Installa Neural Rendering. HDR, MFG e shader sono opzionali; ASI Loader, REFramework e UE4SS servono soltanto alle mod che li richiedono.

### Una modifica alla volta

Non sovrapporre due percorsi di Neural Rendering attivi. Risolvi i conflitti segnalati prima di continuare. Esempio: installa ReShade, controlla che il menu si apra, poi aggiungi NR e confronta la stessa scena. Conserva una configurazione funzionante prima di provare altre funzioni.

## 05 · OptiScaler Standard e F5 · Janblade

Il selettore propone soltanto OptiScaler Standard e F5 · Janblade. La build installata e l’ultima versione verificata sono mostrate separatamente.

### Installare e aggiornare

Scegli la build e usa Installa / cambia build per preparare l’installazione. Controlla aggiornamenti verifica la release della build selezionata. Quando F5 ha una versione più recente, il suo pulsante Aggiorna diventa magenta. Un pacchetto locale non dimostra di essere l’ultima versione online.

### Impostazioni

Salva o applica le impostazioni con il comando dedicato dopo averle modificate. Le funzioni NR multipass e HDR dipendono dal fork e dalla sua versione. Non trattare un parametro salvato come prova dell’effetto attivo. Più passaggi possono aumentare il lavoro della GPU e non garantiscono più FPS.

### Esempio e verifica

Se hai F5 e vuoi aggiornarlo, mantieni selezionato F5 · Janblade, controlla gli aggiornamenti e premi il pulsante della nuova versione. Riavvia il gioco e verifica immagine, menu e fluidità. Per passare a Standard valuta prima quali funzioni del fork perderesti.

## 06 · ReShade: primo passaggio

ReShade è il caricatore usato dal percorso NR tramite add-on e dagli shader opzionali. Installarlo non attiva da solo Neural Rendering o HDR.

### Procedura

1. Chiudi il gioco e selezionalo nella libreria. 2. Apri DLSS 5 · ReShade. 3. Premi Installa ReShade. 4. Leggi l’esito dell’operazione. 5. Avvia il gioco e apri il menu con il tasto configurato. Il tasto si può modificare nei Dettagli di ReShade.

### Canale e loader

Dettagli contiene canale stabile/nightly, importazione e opzioni del menu. Usa la nightly solo se richiesta dal componente o per una prova consapevole. Pulisci loader duplicati è un’azione di manutenzione: non usarla come primo tentativo per ogni errore e leggi le modifiche proposte.

### Esempio

Se il gioco parte ma ReShade non compare, verifica processo di rendering, API e log. Non aggiungere copie dello stesso loader con nomi diversi. Una DLL già presente può appartenere a un’altra mod e l’app può bloccare la sostituzione per conservarla.

## 07 · Neural Rendering: secondo passaggio

Nel percorso ReShade, l’installazione NR prepara l’add-on selezionato e il modello per la GPU quando il rilevamento e i requisiti lo consentono.

### Procedura

1. Completa l’installazione ReShade con supporto add-on. 2. Verifica il percorso Con DLSS nativo oppure il percorso senza DLSS, quando applicabile. 3. Scegli RenoDX NR Lecram 10.8 o ShortFuse dal menu della versione add-on. 4. Premi Installa / aggiorna RenoDX NR. 5. Riavvia il gioco e attiva/verifica NR nel menu ReShade in una scena giocabile.

### Add-on e modello sono diversi

L’add-on integra l’effetto con ReShade; nvngx_dlssnr.dll è il modello. Le varianti incluse 310.9.16.0 distinguono RTX 4000 Compat FP16 / CUDA 13.4.3 e RTX 5000 originale. L’app usa il rilevamento GPU; la gestione separata dei modelli rimane in DLL e Streamline. Se la GPU o i requisiti non sono confermati, risolvi il messaggio prima di installare.

### Esempio e prestazioni

Scegliere ShortFuse cambia l’add-on, non trasforma una GPU in un’altra serie. Parti da una configurazione semplice e confronta FPS e immagine nella stessa scena. NR può ridurre le prestazioni e non garantisce compatibilità universale, HDR o Frame Generation.

## 08 · Giochi senza DLSS, Vulkan ed emulatori

Il supporto dipende dal processo che renderizza e dal percorso d’integrazione disponibile. La presenza di un vecchio gioco o di un launcher non basta a stabilire la compatibilità.

### Feeder e DLSS nativo

Feeder è un percorso distinto per fornire dati a NR in configurazioni senza DLSS supportate. Non aggiungerlo automaticamente a un percorso che già usa DLSS nativo. Segui i requisiti e il messaggio mostrato dall’app: non tutti i giochi senza DLSS diventano compatibili.

### Vulkan, DX11 e DX9

L’app gestisce percorsi Vulkan soltanto per configurazioni riconosciute. Selezionare DirectX 12 nelle opzioni non converte un gioco DX11, DX9 o Vulkan. Un ponte o una mod esterna richiede compatibilità propria; HDLSS5 non certifica un percorso universale per tutte le API.

### Esempio pratico

Con un emulatore avviato da launcher, verifica quale processo carica ReShade e quale gestisce DLSS/FG. Prova prima il launcher senza NR, poi aggiungi un componente alla volta. Il menu iniziale non è una verifica sufficiente: carica una scena giocabile.

## 09 · RenoDX HDR, MFG e shader

Apri Add-on opzionali per le funzioni aggiuntive. Non sono passaggi obbligatori per completare la configurazione NR.

### RenoDX HDR

L’app consulta il catalogo RenoDX e le origini associate alla mod, che possono includere GitHub, siti di download e Nexus. Nessuna mod trovata è diverso da disponibilità non verificata per errore di rete. Per origini/fork che richiedono conferma leggi autore e provenienza. Su Nexus può essere necessario confermare il download nel browser. Importa mod locale serve per pacchetti ottenuti separatamente.

### HDR del gioco

Configura HDR viene proposto soltanto per configurazioni riconosciute. HDR configurato indica le impostazioni preparate, non una misura dell’uscita video. Riavvia e controlla monitor, HDR di Windows e mod nella scena. Evita di sommare conversioni HDR senza istruzioni compatibili.

### MFG e shader

MFG Unlock è distinto da NR e richiede un percorso FG compatibile; non aggiunge automaticamente FG a qualsiasi gioco. Shader ReShade permette di scegliere Lilium oppure CAS e installare il selezionato. Se CAS risulta incluso in Lilium, abilitalo da lì. Esempio: per il solo sharpening scegli CAS, senza installare loader avanzati.

## 10 · DLL e Streamline

Questa scheda gestisce Super Resolution, Ray Reconstruction, Frame Generation, Streamline e il modello Neural Rendering. Sono componenti diversi.

### Aggiornamento

1. Chiudi il gioco. 2. Apri DLL e Streamline. 3. Controlla la versione installata. 4. Seleziona la versione nel menu. 5. Premi Installa o Aggiorna. L’app determina la destinazione; le copie di backup e storage non devono essere scelte come destinazione del gioco. Se il percorso è ambiguo, risolvi il rilevamento dell’eseguibile.

### Catalogo e compatibilità

Il catalogo incluso permette di mostrare scelte anche senza rete; i file non presenti richiedono un download. Controlla aggiornamenti ripete la verifica. Sostituire una DLL aggiorna un’integrazione esistente: non aggiunge DLSS o Frame Generation a un gioco che non li supporta.

### Esempio

Se vuoi aggiornare soltanto Super Resolution, usa il suo riquadro e prova il gioco prima di cambiare Ray Reconstruction o Frame Generation. In caso di problemi usa Ripristina DLL originali nel riquadro interessato.

## 11 · Loader per altre mod

Apri Loader per altre mod · avanzato soltanto quando una mod richiede un loader specifico.

### ASI Loader

Carica file .asi e non installa automaticamente le mod che lo useranno. Scegli il nome DLL indicato dall’autore della mod. L’app verifica il pacchetto x64 e gestisce l’aggiornamento del loader riconosciuto; una DLL di provenienza diversa o un duplicato può bloccare l’operazione.

### REFramework e UE4SS

REFramework è proposto per giochi RE Engine riconosciuti; il pacchetto legacy è diverso da quello corrente quando richiesto. UE4SS è un runtime per mod Unreal. La presenza del motore non garantisce che ogni mod o versione del loader sia compatibile.

### Esempio

Se una mod richiede winmm.dll, non scegliere dinput8.dll soltanto perché il nome è disponibile nel menu. Se il nome è già occupato, identifica il componente esistente prima di sostituirlo. Script e mod esterne possono avere procedure di rimozione proprie.

## 12 · Ripristino, rimozione e backup

Rimuovi elimina un componente gestito; Ripristina recupera una copia conservata. Le due operazioni non sono equivalenti.

### DLL originali

Ripristina DLL originali recupera il backup iniziale salvato dall’app per quel componente e gioco, anche quando sono gestite più copie. Quel backup potrebbe già contenere una modifica precedente all’uso di HDLSS5. Senza backup usa la verifica dei file del launcher per recuperare i file distribuiti dal negozio.

### Configurazione funzionante

Salva una configurazione DLL soltanto dopo averla provata in gioco. Ripristina configurazione salvata torna a quello stato, non necessariamente allo stato iniziale. Conserva la cartella data e le copie di recupero quando aggiorni l’app.

### Esempio

Dopo un aggiornamento che causa problemi, ripristina prima il componente appena cambiato. Non eliminare tutta la cartella del gioco. Rimuovere ReShade può lasciare add-on sul disco che non verranno più caricati: gestisci le rimozioni dai rispettivi riquadri.

## 13 · Aggiornamenti e spazio su disco

Controlla aggiornamenti verifica la disponibilità; l’installazione modifica i file. Un controllo da solo non installa la nuova versione.

### Pulsanti e risultati

Aggiorna è magenta quando è disponibile un aggiornamento verificato. Se non è utilizzabile, è attenuato ma visibile. Errore di rete, versione sconosciuta e componente aggiornato sono stati differenti. Prova un componente alla volta e leggi Attività recenti in caso di errore.

### Cache e modelli

La pulizia automatica limita la cache dei download gestita; i percorsi ancora referenziati possono essere conservati. Il limite della cache non è un limite all’intera cartella dell’app: modelli NR inclusi, backup e file di recupero occupano spazio separato. Non cancellare nr-packages se vuoi usare i modelli inclusi.

### Aggiornare HDLSS5

Chiudi l’app, estrai il pacchetto in una cartella scrivibile e conserva data, backup e nr-packages della copia esistente. Esempio: per una consegna del solo .exe sostituisci l’eseguibile a programma chiuso, senza cancellare le cartelle adiacenti.

## 14 · Problemi e verifica finale

Usa il messaggio esatto e il componente coinvolto per decidere il passo successivo.

### L’effetto non compare

Verifica gioco selezionato, eseguibile di rendering, API, GPU, loader e prerequisiti. Avvia una scena giocabile e apri il menu del componente. Se l’app segnala un’incompatibilità, non forzare una configurazione diversa solo per rendere attivo il pulsante.

### Avvio bloccato o errore

Chiudi il gioco, consulta Attività recenti e i log del componente. Ripristina l’ultima modifica. Le mod esterne all’app possono essere in conflitto e non sono tutte rilevabili. Nei giochi con anti-cheat una sostituzione di DLL può essere bloccata; l’app non garantisce l’accettazione delle mod e non disattiva l’anti-cheat.

### Antivirus e richiesta di assistenza

La precedente segnalazione Defender sull’eseguibile non è stata confermata risolta. Non considerare i test dell’app una certificazione antivirus. Conserva nome della rilevazione, versione dell’app e hash del file per l’analisi del produttore. Per assistenza aggiungi gioco/launcher, GPU, percorso interessato, versioni, azione e messaggio completo, senza credenziali.

## 15 · Novità della 1.5.1

28 ottobre 2026. Guida riscritta seguendo l’ordine d’uso: interfaccia, libreria, configurazione, componenti, ripristino e risoluzione dei problemi.

### Interfaccia

Azioni di installazione, rimozione e Dettagli allineate nelle schede affiancate. Tipografia e spaziature uniformate. Descrizione degli shader nei Dettagli. Pulsanti di aggiornamento magenta quando è disponibile una nuova versione; comandi disabilitati attenuati.

### OptiScaler

Scelta limitata a Standard e F5 · Janblade. Aggiornamento F5 più visibile e riconoscimento dei pacchetti con prefisso v nella versione. Guida aggiornata ai comandi e ai percorsi attuali, senza istruzioni per fork rimossi.

### Verifiche e limiti

Compilazione e test di regressione verificano il comportamento dell’app; le prove UI verificano presentazione e allineamento. Compatibilità in gioco, prestazioni e classificazione antivirus richiedono verifiche separate.
