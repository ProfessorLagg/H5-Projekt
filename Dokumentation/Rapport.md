<style>
:root {
    --a4w: 210mm;
    --a4h: 297mm;
    --a4wu: calc(var(--a4w) / 100);
    --a4hu: calc(var(--a4h) / 100);
    --a4mu: calc(max(var(--a4wu), var(--a4hu)));
}
* {
    margin: 0px;
    padding: 0px;
    font-family: 'Arial';
    max-width: var(--a4w);
}
body{
    background-color: magenta;
    color: black;
    width: var(--a4w);
    min-height: var(--a4h);
    display: flex;
    flex-direction: column;
    
}
section{
    padding: 1cm;
    background-color: white;
    width: var(--a4w);
    min-height: var(--a4h);
    page-break-after: always;
    outline: 1px solid black;
}

h1{font-size: 48px;}
h2{font-size: 36px;}
h1{font-size: 48px;}
h1{font-size: 48px;}

h1, h2 {
    text-decoration: none;
}
.center {
    text-align: center;
    justify-self: center;
}

</style>
<section id="forside">
<div class="center">

# Laggdoku <!-- omit from toc -->
## Processrapport <!-- omit from toc -->

*2025.05.22 | Søren P.K Thuesen | TEC Ballerup*

</div>
</section>

<section>

# Indholdsfortegnelse <!-- omit from toc -->
- [Forord](#forord)
  - [Formål](#formål)
  - [Hvem er jeg](#hvem-er-jeg)
- [Læsevejledning](#læsevejledning)

</section>

<section>

# Læsevejledning
Denne rapport beskriver selve udviklingsforløbet og de teknologiske valg der er blev taget og
begrundelse herfor.
Kildekoden for frontend og backend er tilgængelig på GitHub. Links hertil findes i ”Bilag 7 –
Kildekoden”. 
</section>


<section>

# 1 Forord
Formålet med denne rapport er at give læseren en dybere indsigt i Laggdoku's udvikling.
Laggdoku er udviklet af Søren P.K Thuesen.
Denne rapport indeholder beskrvelse af både produktet og udviklingsprocessen.
### 1.1 Formål
Laggdoku er en fusion imellem tetris og sodoku, hvor brikker placeres på et 9x9 gitter.
Brikkerne er en samling af blokke, som hver især fylder et en celle i 9x9 gitteret, og tager formæssig udgangspunkt i tetris-brikkerne.
Når en række, kollonne eller 3x3 under gitter er helt fyldt med blokke bliver den ryddet.
Spilleren optjener score når en blok placeres, og når den ryddes.
Hver runde gives spilleren 3 brikker, som må placeres i vilkårlig rækkefølge.
Splittet slutter hvis splilleren ikke kan placere en brik, dvs. når der ikke er plads i gitteret til en brik.
Når et spil er slut vil spilleren blive givet en endelig score, og få muligheden for at uploade resultatet til et online leaderboard.
### 1. 2Hvem er jeg 
Mit navn er Søren P.K Thuesen
Jeg har 4+ års erfaring som Teknisk Konsulent hos D4 ApS, hvor jeg har arbejdet hovedesageligt med 
	- IIS hostede ASP Hjemmesider
	- Microsoft SQLServer
	- Udvikling af PowerShell baseret IIS Sites overvågning
	- Drift af Windows Server
	- Udvikling og vedligehold af C# og PowerShell baserede værktøjer til opdatering og validering af D4's produkter
	
Gennem min læreplads hos D4 ApS har jeg opnået praktisk erfaring inden for blandt andet Microsoft SQL Server, IIS, ASP (både .net framework, VBScript og JScript), PowerShell, Drift af Windows Server og C# udvikling
Derudover går meget af min fritid på software udvikling, hvorigennem jeg har fået erfaring med bla. Zig, Go, x86 Assembly, Godot og meget mere
Jeg har en forkærlighed for performance optimering, hvorigennem mange af mine fritidsprojekter har handlet udelukkende om performance opdatering
### Indledning

</section>

<section>

# Læsevejledning
TODO
</section>



<section>

# Bilag
### 1. Kildekode
</section>



