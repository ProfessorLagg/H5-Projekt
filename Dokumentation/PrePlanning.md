Kontrol & Simplicitet, Legacy Support, Cross-Platform, Fremtidssikret


# Teknologier
## Frontend
Statisk web site med WebComponents, med fokus på så få filer som muligt (gerne en enkelt).
**Begrundelse:**
- |Performance| Såfremt jeg kan pakke det hele ned i èn fil, kræves der kun èn request for at hente hele sitet.
- |Performance| Der sendes ikke noget frontend-framework ned, hvilket vil sige at al koden der sendes bruges
- |Legacy Support, Cross-Platform| WebComponents virker i alle browsere, selv meget gamle browsere
- |Performance, Kontrol & Simplicitet| WebComponents lader mig bestemme selv, jeg er ikke bundet til hvad der kan / ikke kan lade sig gøre i et front-end framework
- |Kontrol & Simplicitet, Cross-Platform| Denne løsning sætter ingen krav eller forventninger til min webserver implementation, ud over et minimumskrav på HTTP 1.1.
## Backend
Simpel C# WebServer via. HttpListener og Sql.Data.SqlClient
**Begrundelse:**
- |Performance, Kontrol & Simplicitet| Denne løsning minimere mængden af framework kode (relativt til f.eks ASP.NET Core og EntityFramework), og minimere derved mængden af unødvændig kode eksekvering, hvilket giver bedre performance.
- |Performance, Kontrol & Simplicitet| Sql.Data.SqlClient lader mig bestemme hvordan både databasen og min webserver er struktureret. Hvilket lader mig styre performance hvor det er nødvændigt og/eller nyttigt
### Database
SQLite
**Begrundelse:**
- Simpleste og mest performant "off the shelf" løsning
    - Det ville nok være simplere og mere performant selv at skrive en databaseløsning, men det er et stort stykke arbejde, og temmelig error prone
    - Tiger Beetle er mere performant, næsten lige så simpel og nok mere reliable. Men jeg har ingen erfaring med Tiger Beetle.

- Gør testing med falsk data nemmere, da jeg bare kan pege på en eksisterende.
- Gør setup og 