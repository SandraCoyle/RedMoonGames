# Rød Måne · Spillerummet

Mobilvenlig match-3 prototype på dansk. Match mindst 3 ens symboler, og saml så mange point som muligt på 28 træk. Runden slutter først, når de 28 træk er brugt; 1.000 point stopper ikke spillet. Cykluslængder varierer; 28 træk er et spiltema. 3 på række giver 60 point, 4 giver 120, og 5 giver 300. Længere rækker giver 20 pr. felt plus 200 i bonus. Alle nabobyt bruger ét af de 28 træk. Et byt uden match trækker 20 point fra (også under nul), og brikkerne bliver på deres nye pladser. Spillepladen blandes ikke automatisk; du kan flytte brikker for at bygge et senere match. Kæder ganger pointene med kædens nummer. Understøtter tryk, swipe og tastaturets Tab/Enter. Ingen login, tracking eller backend. Personlig rekord (højeste opnåede score), vundet stjernekrone og om kronen er taget på katten gemmes lokalt i browseren.

## Unity 6000.6.4f1

Åbn repositoryets rod i Unity Hub. Startscenen er `Assets/RedMoonGames/Scenes/Spillerummet.unity`. Se [Unity-vejledningen](docs/UNITY.md) for import, Play, Android og iOS. Unity-udgaven bruger native UI Toolkit og C#; ingen WebView er nødvendig. C#-spilleregler er testet mod browserversionen. Editorimport og mobilbuild er endnu ikke afprøvet i dette miljø.

## Browserudvikling

Kræver Node.js 20 eller nyere. Spillet har ingen runtime-pakker. Browserchecks og grafikeksport bruger Playwright som udviklingsafhængighed (`npm ci`).

```sh
npm start
npm test
```

Serveren bruger port 3000 (kan ændres med `PORT`). Åbn spillet i en browser. `engine.js` indeholder spilleregler, `game.js` brugerfladen, og `style.css` designet. Google Fonts bruges som valgfri typografi med lokale fallback-skrifttyper.

Prototypen er browserbaseret. Der findes nu også en selvstændig Unity-udgave, se ovenfor. Integration i en eksisterende Rød Måne app er ikke implementeret. Teksterne er generel information og bør fagligt gennemgås før udgivelse.

## Spil uden server

Kør `node scripts/export.js` for at oprette `dist/Cyklusmatch.html`. Den selvstændige HTML-fil indeholder design, spilleregler og brugerflade og kan åbnes direkte i en browser uden netforbindelse. På telefoner afhænger åbning af lokale HTML-filer af browseren; en hostet webversion er bedst til deling.

GODT AT VIDE OM MENSTRUATION viser 20 praktiske og trygge tips til unge omkring deres første menstruation i blandet rækkefølge uden gentagelser, indtil alle er vist. Fakta skifter ved gyldige matches, en ny runde og knappen “Næste tip”.

En enkelt træktæller viser 28 → 0. Fem eller flere på række skaber en stjerne på pladen, også når en aktiv joker indgår. Stjerner matcher alle symboler, bruges én gang og giver +120 ekstra ved fire på række eller +300 ved fem eller flere. Jokere er passive under kædereaktioner og aktiveres af et match lavet med spillerens eget byt, også når en almindelig brik flyttes ind til en stjerne; kædemultiplikatoren gælder også stjernebonus.

Matchende rækker, der deler en brik (L/T/kryds), tæller som én gruppe. 3+3 med en fælles brik er fem unikke brikker og giver fem-bonus og en ny stjerne. Adskilte rækker tælles separat.

Tre stjerner på række, som aktiveres af spillerens eget byt, giver 3.000 jackpotpoint; hver ekstra stjerne i rækken giver +1.000. Eventuelle tilstødende almindelige matches tælles oveni. Rene stjerner aktiveres ikke automatisk i kæder. Hjertebrikken er erstattet af en tegnet menstruationskop.

## Rekord og avatarbelønning

Over 5.000 point i én runde låser en stjernekrone op. På slutskærmen kan du trykke på kronen: katteavataren kommer ind og får kronen på. Præcis 5.000 er ikke nok. Belønningen mistes ikke ved senere minuspoint eller nye runder. Kronen kan tages af/på. Data gemmes i localStorage under `cyklusmatch.profile.v1`; ingen konto eller synkronisering mellem browsere/enheder. Slettede browserdata fjerner også rekord og belønning. Lokale HTML-filers lagring afhænger af browseren og filplaceringen; brug samme fil/browser. Hvis lagring blokeres, vises en besked, og spillet virker stadig i den aktuelle session. Integration med Rød Måne-appens eksisterende avatar/konto er endnu ikke implementeret.

Katteavataren er en tegnet prototype. Kroneforløbet respekterer reduceret bevægelse. Belønningen låses op under spillet, men tages først på ved tryk på slutskærmens krone eller via avatarens knap. Eksisterende gemte kroner og rekorder bevares.

## Spilmenu

Spillet åbner i Spillerummet. Cyklusmatch er spilbart; Husk & match og Månefangst er kun idéer markeret “Kommer senere”. Spilkortene beskrives i `games` i `game.js`. Tilføj først et aktivt kort, når det tilhørende spil og navigation er implementeret. Katteavatar og lokal profil deles mellem menu og spil. “Alle spil” og logoet åbner menuen; “Fortsæt runden” bevarer den aktuelle spilleplade, score og resterende træk, mens siden er åben. Genindlæsning starter en ny runde, men rekord og belønninger bevares i samme browser. Slutskærmen har også en knap til menuen.

Menuens avatarboks er generel og viser katten på tværs af spil. Kronemål, fremgang, kronemeddelelse og knappen til at tage kronen på/af vises kun i Cyklusmatch. En allerede båret krone ses stadig på katten i menuen.

## Checks

`npm test` kører spilleregel- og profiltests. `npm run export` opdaterer den selvstændige HTML-fil. Efter `npm ci` og `npx playwright install chromium` køres browserchecks med `CHROMIUM_PATH` sat til Chromium-eksekverbaren og `npm run test:browser`. Unity-kernen kontrolleres med `dotnet run --project tests/unity-core/UnityCoreChecks.csproj`. GitHub Actions er konfigureret til begge.
