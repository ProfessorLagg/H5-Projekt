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
C# .NET HttpListener baseret webserver

### Database
SQLite
.NET bibliotek System.Data.SqlClient benyttes til at kommunikere med databasen




