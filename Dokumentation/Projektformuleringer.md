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
- Den nuværende score skal tydeligt fremgå
- Det skal fremgå af ikke placerede brikker hvorvidt de kan placeres på brættet eller ej
- Spilleren præsenteres for 3 brikker af gangen, som alle sammen skal placeres på brættet før 3 nye brikker præsenteres.
- Når nye brikker generes skal mindst 1/3 af brikkerne kunne placeres på brættet
- 

## 

## Must Have

- Brugerne skal kunne "drag n drop" brikkerne på brættet
- Spillet skal give brugeren point for hver blok placeret og ryddet

- Når spilleren gives nye brikker, skal mindst èn af disse brikker kunne placeres på brættet, så det altid er en brugerhandlig som resultere i at et spil slutter
- Applikationen skal kun sende netværkstrafik hvor nødvændigt for at kommunikere med servere
- Skal kunne virke i følgende browsere: Chrome til Android, Chrome til Windows, Safari på iOS
- Spillet skal ikke justere sværhedsgraden, så high-scores altid forbliver sammenlignelige
- Når en bruge slår sin personlige highscore skal de have mulighed for at uploade den til leaderboard
- En brugers score skal kunne rankeres og vises på leaderboard
- Det skal være muligt at genoptage ikke-færdiggjorte spil
- Spillet skal slutte når en bruger løber tør for placer-bare brikker.
- Brugeren skal præsenteres for 3 brikker hver runde, hvoraf mindst èn kan placeres.

## Should have
- Maks responstid på alle handlinger skal være under 1 sekund
- Mulighed for at uploade spil til serveren, og få dem vist på et leaderboard

## Could have
- Mulighed for at logge ind og beholde sin highscore på tværs af enheder
- Mulighed for at brugeren customize tema og farver

## Won't have
- Al funktionalitet skal kunne tilgås "anonymt". Altså uden at logge ind.
