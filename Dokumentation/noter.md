# Kravspec Checklist
## Performance Krav
En vigtig del af spiloplevelsen er performance. Hvis performance halter og brugeren f.eks skal vente længe på at spillet loader eller kommunikere med serveren, så er spillet simpelthen mindre sjovt.
Der stilles derfor følgende krav til performance:
- [ ] Der må ikke forkomme ventetider for brugeren på over `1000 ms`, og skal i gennemsnit være under `20ms`
- [ ] Web-Responstid må ikke overstige `ping + 1000ms`, og skal i gennemsnit være under `ping + 20ms`

## Krav til gameplay
- [ ] Det skal være muligt at starte et nyt spil, også selvom du har gang i et eksisterende
- [x] Brikkerne skal kunne trækkes ud på brættet, både på mobil og desktop
- Før en brik placeres skal det tydeligt fremgå:
  - [x] Præcist hvilke celler som brikken kommer til at fylde
  - [ ] Hvorvidt en række/kolonne/gruppe vil blive ryddet som resultat
- [x] Den nuværende score skal tydeligt fremgå af brugerfladen
- [ ] Det skal fremgå af ikke placerede brikker hvorvidt de kan placeres på brættet eller ej
- [x] Spilleren præsenteres for 3 brikker af gangen, som alle sammen skal placeres på brættet før 3 nye brikker præsenteres.
- [x] Kan splilleren ikke placere en brik ikke placeres er spillet slut
- [x] Når nye brikker generes skal mindst 1 af de 3 nye brikker kunne placeres på brættet
- [ ] Når en bruge slår sin personlige highscore skal de have mulighed for at uploade den til leaderboard
- [x] Det skal være muligt at genoptage ikke-færdiggjorte spil
- [x] Spillet skal huske brugerens high-score
- [ ] Det skal være muligt for brugeren at vælge et username / tag, som vises på leaderboard.

## Krav til teknologi
- [x] Spil-siden skal kunne hentes i sin helhed, så der ikke kræves internet i løbet af spillet
- [x] Tilfældigheden i spillet skal være baseret på et seed
- [ ] Når en ny high-score uploades skal WebServeren kunne validere at alle træk taget i løbet af spillets gang var tilladte.
- [ ] Spillet skal kunne encodes på en deterministisk måde, så det kan genspilles og valideres. *(Replay Encoding)*
- [ ] Databasen skal opbevarer leaderboards over high-scores
- [ ] Databasen skal opbevarer replays af spil, med tilknyttet username og tidspunkt
- [ ] Replay Encoding formatet skal som minimum indeholde information omkring følgende:
  - [ ] Format Version
  - [ ] Tilfældigheds Seed
  - [ ] Tilfældigheds Algoritme
  - [ ] De træk som brugeren har taget, i den rækkefølge de tog dem
- [ ] Spillet skal kunne spilles i følgende browsere: Chrome til Android, Chrome til Windows, Safari på iOS

