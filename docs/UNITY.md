# Unity · Rød Måne Spillerummet

Projektroden er samme mappe som denne repositories `Assets`, `Packages` og `ProjectSettings`.
Editorversionen er låst til **6000.6.4f1** i `ProjectSettings/ProjectVersion.txt`, som ønsket. Editorrevision er ikke gættet eller tilføjet. Brug præcis den version i Unity Hub.

## Åbn og spil

1. Installér Unity **6000.6.4f1** via Unity Hub med Android Build Support (inklusive SDK, NDK og OpenJDK), hvis du vil teste på Android. iOS kræver iOS Build Support, en Mac og Xcode.
2. Vælg **Add project from disk** i Hub, og vælg repositoryets rodmappe.
3. Lad Unity importere projektet. Der er ingen Asset Store-pakker eller eksterne runtime-pakker. UI Toolkit og de øvrige pakker i manifestet er indbyggede Unity-moduler.
4. Åbn **RedMoonGames → Open Spillerummet**, eller åbn `Assets/RedMoonGames/Scenes/Spillerummet.unity`.
5. Vælg **RedMoonGames → Configure mobile project** én gang. Det angiver portrait, appnavn, eksempelpakkens id og startscene. Unity genererer resten af sine projektindstillinger ved første import.
6. Vælg **RedMoonGames → Validate project**, og tryk derefter **Play**. Game-vinduet kan sættes til 390 × 844 eller en anden telefonstørrelse.

Sceneobjektet er med vilje minimalt. `RedMoonApp.Bootstrap` opretter native UI Toolkit, menu, spil og lokal profil, når Play starter. Der skal ikke monteres scripts, prefabs eller Inspector-referencer manuelt. Runtime kræver hverken en browser eller en WebView.

## Hvad er med?

- Spilmenu med Cyklusmatch og to inaktive pladser til senere spil.
- Native 7×7-spilleplade med klik/tryk og swipe, 28 træk, varige byt og −20 for intet match.
- Matchbonusser, L/T-match, jokere, passive stjerner i kædereaktioner og jackpot.
- De samme 20 danske tips til unge omkring første menstruation.
- Transparent grafik for dråbe, måne, blomst, tampon, bind, kop, stjerne, kat og krone.
- Lokal rekord og belønning via PlayerPrefs. Over 5.000 point låser kronen op; på slutskærmen kan du trykke på den, så katten kommer ind og får den på.
- Native menuer til Android-APK og iOS-Xcode-eksport.

Rekord og kattebelønning er lokale i den enkelte installation. Browserversionen bruger localStorage og har sin egen separate profil. Der er ingen synkronisering, konto eller integration med en eksisterende Rød Måne-app/avatar endnu. Spilrunder gemmes ikke ved lukning; rekord og krone gemmes.

## Byg til telefon

**Android:** Tilslut en Android-telefon med USB debugging, og vælg **RedMoonGames → Build → Android development APK**. Output: `Builds/Android/RedMoonGames.apk`. Brug `adb install -r Builds/Android/RedMoonGames.apk`, eller installér APK'en på din egen testtelefon. Det er en development-APK med standard debug-signering, ikke en Play Store-udgivelse.

**iOS:** På en Mac vælges **RedMoonGames → Build → iOS Xcode project**. Åbn det genererede projekt under `Builds/iOS` i Xcode, vælg dit eget signing team og test på en iPhone.

Eksempel-id'et `dk.redmaane.games` skal erstattes med organisationens eget id før udgivelse. Store-signering og publicering er ikke konfigureret.

## Verifikation og begrænsninger

C#-kernen er kompileret og kørt med .NET 8 uden Unity-afhængigheder. 27 fixtures sammenligner native scoring, matchfelter og nye stjerner direkte med JavaScript-versionen; derudover testes friske plader, alle 28 træk, jackpot og profilregler.

```sh
node scripts/export-unity-fixtures.mjs
dotnet run --project tests/unity-core/UnityCoreChecks.csproj
```

**Unity-editoren er ikke installeret i opsætningsmiljøet.** Native UI, sceneimport, PlayerPrefs, mobile touch-events, katteanimationen og Android/iOS-buildmenuerne er implementeret, men ikke kørt i Unity 6000.6.4f1 eller på en fysisk telefon. .NET-testene validerer ikke Unity-API-kompatibilitet eller mobilbuilds. Den afsluttende editor- og enhedskontrol skal derfor udføres ovenfor, før udgaven kan kaldes buildvalideret.

Manuel tjekliste i Unity: åbn menuen → spil → byt både med og uden match → brug en joker → forlad og fortsæt runden → brug alle 28 træk → se slutskærmen. Ved over 5.000 point: tryk krone, se katten, luk og genstart Play og bekræft rekord/krone. Test en APK på en telefon for safe area og swipe.

## Struktur

- `Assets/RedMoonGames/Scripts/Core/`: ren C#-spilmotor og profilregler.
- `Assets/RedMoonGames/Scripts/RedMoonApp.cs`: native UI, navigation, touch, lagring og belønningsforløb.
- `Assets/RedMoonGames/Editor/ProjectSetup.cs`: opsætning, validering og buildmenuer.
- `Assets/RedMoonGames/Resources/`: tema, tips og PNG-grafik.
- `Assets/RedMoonGames/Scenes/Spillerummet.unity`: indgangsscene.
- `tests/unity-core/`: selvstændige .NET-tests. Intet NuGet-testframework er nødvendigt.

Browserversionen bevares i repositoryets rod. De to motorer er separate; kør parity-checks efter ændringer i reglerne. Ved et nyt spil skal menu, spiltilstand og en særskilt belønning implementeres i begge platforme, hvis begge fortsat skal understøttes.
