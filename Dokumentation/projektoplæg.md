# Tema / Titel
Webbaseret Spil med stærk inspiration fra Woodoku

## Case Beskrivelse
Spillet er en fusion imellem tetris og sodoku.
Spilleren placere brikker i et 9x9 gitter.
Brikkerne er en samling af blokke, som hver især fylder et en celle i 9x9 gitteret, og tager formæssig udgangspunkt i tetris-brikkerne.
Når en række, kollonne eller 3x3 under gitter er helt fyldt med blokke bliver den ryddet.
Spilleren optjener score når en blok placeres, og når den ryddes.
Hver runde gives spilleren 3 brikker, som må placeres i vilkårlig rækkefølge.
Splittet slutter hvis splilleren ikke kan placere en brik, dvs. når der ikke er plads i gitteret til en brik.
Når et spil er slut vil spilleren blive givet en endelig score, og få muligheden for at uploade resultatet til et online leaderboard.

## Teknologi
### Frontend
Statisk Web-Side bygget med HTML, CSS & JavaScript m. WebComponents

### Backend Webserver
C# [System.Net.HttpListener](https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener) baseret webserver

### Database
SQLite
.NET bibliotek [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite) benyttes til at kommunikere med databasen

<style>body{width: 210mm;}</style>
# Problemformulering
I min familie har vi over de sidste par år haft en hygge-konkurence kørrende i hvem der kan få den højeste score i spillet Woodoku. Min mor desværre udelukket fra kunkorrencen da Woodoku ikke er cross-platform og kun virker på Android.
Spillet er derudover fyldt med abrydelser og lange ventetider i form af reklame-bombadament, pop-ups, irriterende forsøg på "Engagement Farming" og et hav af Performance problemer, som tilsammen resultere i en unødvændigt frusterende spiloplevelse.
Den interne hygge-konkurence er også forværret af at Woodoku, uden at informere brugeren, justere sværhedsgraden. Hvilket gør at high-scores ikke er sammenlignelige

Mit projekt går derfor ud på at bygge et cross-platform alternativ, fri for de frustrationspunkter som findes i Woodoku:
- Kan det lade sig gøre, via. simple web-teknologier, at bygge et cross-platform alternativ til Woodoku?
- Kan jeg levere en bedre spiloplevelse, fri for abrydelser og lange ventetider?
- Er det muligt at levere en mere tilgængelig og sammenlignelig konkurrence?


# Kravspecifikation

## Performance Krav
En vigtig del af spiloplevelsen er performance. Hvis performance halter og brugeren f.eks skal vente længe på at spillet loader eller kommunikere med serveren, så er spillet simpelthen mindre sjovt.
Der stilles derfor følgende krav til performance:
- Der må ikke forkomme ventetider for brugeren på over `1000 ms`, og skal i gennemsnit være under `20ms`
- Web-Responstid må ikke overstige `ping + 1000ms`, og skal i gennemsnit være under `ping + 20ms`

## Krav til gameplay
- Det skal være muligt at starte et nyt spil, også selvom du har gang i et eksisterende
- Brikkerne skal kunne trækkes ud på brættet, både på mobil og desktop
- Før en brik placeres skal det tydeligt fremgå præcist hvilke celler som brikken kommer til at fylde, samt hvorvidt en række/kolonne/gruppe vil blive ryddet som resultat
- Den nuværende score skal tydeligt fremgå af brugerfladen
- Det skal fremgå af ikke placerede brikker hvorvidt de kan placeres på brættet eller ej
- Spilleren præsenteres for 3 brikker af gangen, som alle sammen skal placeres på brættet før 3 nye brikker præsenteres.
- Kan splilleren ikke placere en brik ikke placeres er spillet slut
- Når nye brikker generes skal mindst 1 af de 3 nye brikker kunne placeres på brættet
- Når en bruge slår sin personlige highscore skal de have mulighed for at uploade den til leaderboard
- Det skal være muligt at genoptage ikke-færdiggjorte spil
- Spillet skal huske brugerens high-score
- Det skal være muligt for brugeren at vælge et username / tag, som vises på leaderboard.

## Krav til teknologi
- Spil-siden skal kunne hentes i sin helhed, så der ikke kræves internet i løbet af spillet
- Tilfældigheden i spillet skal være baseret på et seed
- Når en ny high-score uploades skal WebServeren kunne validere at alle træk taget i løbet af spillets gang var tilladte.
- Spillet skal kunne encodes på en deterministisk måde, så det kan genspilles og valideres. *(Replay Encoding)*
- Databasen skal opbevarer leaderboards over high-scores
- Databasen skal opbevarer replays af spil, med tilknyttet username og tidspunkt
- Replay Encoding formatet skal som minimum indeholde information omkring følgende:
  - Format Version
  - Tilfældigheds Seed
  - Tilfældigheds Algoritme
  - De træk som brugeren har taget, i den rækkefølge de tog dem
- Spillet skal kunne spilles i følgende browsere: Chrome til Android, Chrome til Windows, Safari på iOS
