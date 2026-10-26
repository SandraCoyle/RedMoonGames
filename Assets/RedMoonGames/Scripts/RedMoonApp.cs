using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedMoonGames
{
    public sealed class RedMoonApp : MonoBehaviour
    {
        const string SaveKey = "RedMoonGames.profile.v1";
        static readonly CultureInfo Danish = CultureInfo.GetCultureInfo("da-DK");
        static readonly string[] TileIcons = { "drop", "moon", "flower", "tampon", "pad", "cup", "star" };
        static readonly string[] TileNames = { "Dråbe", "Måne", "Blomst", "Tampon", "Bind", "Kop", "Joker" };
        static readonly string[] TileColors = { "#f3d9df", "#e6def1", "#f6e2da", "#dce9ed", "#ede4d5", "#edd9e4", "#ffedb8" };
        readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        PlayerProgress profile;
        MatchGame game;
        VisualElement root, content, overlay;
        ScrollView scroll;
        PanelSettings panel;
        bool playing, busy, saveFailed;
        int selected = -1;
        string message = "Byt to nabobrikker. Lav mindst 3 ens på række.";
        string tip;
        readonly List<string> tips = new List<string>();
        readonly List<string> remainingTips = new List<string>();
        readonly System.Random random = new System.Random();
        [Serializable] sealed class TipCollection { public string[] items; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (FindFirstObjectByType<RedMoonApp>() == null)
            {
                var host = new GameObject("RedMoonGames");
                DontDestroyOnLoad(host);
                host.AddComponent<RedMoonApp>();
            }
        }

        void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            profile = Load();
            var text = Resources.Load<TextAsset>("Tips");
            if (text != null)
            {
                var collection = JsonUtility.FromJson<TipCollection>(text.text);
                if (collection != null && collection.items != null) tips.AddRange(collection.items);
            }
            if (tips.Count == 0) tips.Add("Spørg en voksen, du stoler på, når du er i tvivl.");
            NextTip();
            game = new MatchGame();
            var document = gameObject.AddComponent<UIDocument>();
            panel = ScriptableObject.CreateInstance<PanelSettings>();
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(390, 844);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 0;
            panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("RuntimeTheme");
            document.panelSettings = panel;
            root = document.rootVisualElement;
            root.style.flexGrow = 1;
            root.style.backgroundColor = Hex("#faf6f2");
            scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            float units = 390f / Mathf.Max(1, Screen.width);
            scroll.style.paddingTop = (Screen.height - Screen.safeArea.yMax) * units;
            scroll.style.paddingBottom = Screen.safeArea.yMin * units;
            root.Add(scroll);
            content = new VisualElement();
            content.style.paddingLeft = content.style.paddingRight = 14;
            content.style.paddingTop = content.style.paddingBottom = 18;
            scroll.Add(content);
            Render();
        }

        PlayerProgress Load()
        {
            try
            {
                var value = JsonUtility.FromJson<PlayerProgress>(PlayerPrefs.GetString(SaveKey, "{}")) ?? new PlayerProgress();
                value.Validate(); return value;
            }
            catch (Exception) { return new PlayerProgress(); }
        }

        void Save()
        {
            try { PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(profile)); PlayerPrefs.Save(); saveFailed = false; }
            catch (Exception) { saveFailed = true; }
        }

        void NextTip()
        {
            if (remainingTips.Count == 0)
            {
                remainingTips.AddRange(tips);
                for (int i = remainingTips.Count - 1; i > 0; i--)
                { int j = random.Next(i + 1); string item = remainingTips[i]; remainingTips[i] = remainingTips[j]; remainingTips[j] = item; }
                if (remainingTips.Count > 1 && remainingTips[0] == tip)
                { string item = remainingTips[0]; remainingTips[0] = remainingTips[1]; remainingTips[1] = item; }
            }
            tip = remainingTips[0]; remainingTips.RemoveAt(0);
        }

        static Color Hex(string value) { ColorUtility.TryParseHtmlString(value, out var color); return color; }
        static string Number(int value) => value.ToString("N0", Danish);
        static void Round(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = element.style.borderBottomRightRadius = radius;
        }
        VisualElement Box(string color, float padding = 16)
        {
            var box = new VisualElement(); box.style.backgroundColor = Hex(color);
            box.style.paddingTop = box.style.paddingBottom = padding;
            box.style.paddingLeft = box.style.paddingRight = padding;
            box.style.marginBottom = 14; Round(box, 18); return box;
        }
        static VisualElement Row()
        { var row = new VisualElement(); row.style.flexDirection = FlexDirection.Row; row.style.alignItems = Align.Center; return row; }
        Label Text(string value, int size = 14, string color = "#704354")
        {
            var label = new Label(value); label.style.fontSize = size; label.style.color = Hex(color);
            label.style.whiteSpace = WhiteSpace.Normal; label.style.marginBottom = 7;
            label.style.flexShrink = 1; return label;
        }
        Button Action(string label, System.Action callback, bool secondary = false)
        {
            var button = new Button(callback) { text = label };
            button.style.minHeight = 44; button.style.fontSize = 14;
            button.style.whiteSpace = WhiteSpace.Normal;
            button.style.backgroundColor = Hex(secondary ? "#f0e3e8" : "#7c304a");
            button.style.color = Hex(secondary ? "#7c304a" : "#fffaf7");
            button.style.borderTopWidth = button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = button.style.borderRightWidth = 0;
            button.style.marginTop = 8; Round(button, 23); return button;
        }
        Image Icon(string name, float width, float height)
        {
            if (!textures.TryGetValue(name, out var texture))
            { texture = Resources.Load<Texture2D>("Icons/" + name); textures[name] = texture; }
            var image = new Image { image = texture, scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
            image.style.width = width; image.style.height = height; image.style.flexShrink = 0; return image;
        }

        void Render()
        {
            Vector2 offset = scroll.scrollOffset;
            content.Clear();
            var header = Row();
            var home = Action("RØD MÅNE", () => { if (!busy) OpenMenu(); }, true);
            home.style.flexGrow = 1; header.Add(home);
            if (playing)
            {
                var help = Action("?", ShowHelp, true); help.style.width = 44; header.Add(help);
            }
            header.style.marginBottom = 20; content.Add(header);
            if (playing) BuildGame(); else BuildMenu();
            BuildAvatar();
            if (playing)
            {
                var fact = Box("#f3ece3");
                fact.Add(Text("GODT AT VIDE OM MENSTRUATION", 10, "#967653"));
                fact.Add(Text(tip, 13, "#806b60"));
                fact.Add(Action("Næste tip", () => { NextTip(); Render(); }, true)); content.Add(fact);
            }
            content.Add(Text(saveFailed ? "Kan ikke gemme lige nu. Dine ting er stadig her, mens spillet er åbent."
                : "Rekord og belønninger gemmes på denne enhed.", 10, "#a08892"));
            content.Add(Text("SPIL I DIT EGET TEMPO", 10, "#a08892"));
            scroll.schedule.Execute(() => scroll.scrollOffset = offset);
        }

        void BuildMenu()
        {
            content.Add(Text("DIT SPILLEPUSTERUM", 10, "#a07787"));
            content.Add(Text("Lidt leg.\nHelt din rytme.", 37, "#7c304a"));
            content.Add(Text("Vælg et spil, tag en pause, og saml fine ting til din kat.", 14, "#987a85"));
            content.Add(Text("Dine spil", 23));
            var card = Box("#f4e2e8", 20);
            var art = Row(); foreach (string name in new[] { "drop", "moon", "flower", "star" }) art.Add(Icon(name, 45, 45)); card.Add(art);
            card.Add(Text("MATCH-3 · 28 TRÆK", 10, "#9b6c80")); card.Add(Text("Cyklusmatch", 26));
            card.Add(Text("Match symboler, skab stjerner, og slå din egen rekord."));
            card.Add(Text("Din rekord: " + Number(profile.bestScore) + " point", 12));
            card.Add(Action(game.Moves == 0 ? "Spil igen" : game.Moves < 28 ? "Fortsæt runden" : "Spil Cyklusmatch", () =>
            { if (game.Moves == 0) game = new MatchGame(); playing = true; selected = -1; Render(); scroll.scrollOffset = Vector2.zero; }));
            content.Add(card);
            foreach (var title in new[] { "Husk & match", "Månefangst" })
            {
                var later = Box("#f1e9e8"); later.Add(Text(title, 22, "#a0808d"));
                later.Add(Text("Kommer senere", 12, "#a0808d")); content.Add(later);
            }
        }

        void BuildAvatar()
        {
            var box = Box("#fff5e6"); var row = Row();
            var portrait = new VisualElement(); portrait.style.width = 78; portrait.style.height = 95; portrait.style.flexShrink = 0;
            var cat = Icon("cat", 74, 87); cat.style.position = Position.Absolute; cat.style.bottom = 0; portrait.Add(cat);
            if (profile.crownEquipped)
            {
                var crown = Icon("crown", 45, 30); crown.style.position = Position.Absolute;
                crown.style.top = 0; crown.style.left = 14; portrait.Add(crown);
            }
            row.Add(portrait);
            var words = new VisualElement(); words.style.flexGrow = 1; words.style.flexShrink = 1;
            words.Add(Text("DIN KAT", 10, "#9d6c7c"));
            words.Add(Text(!playing ? "Med i alle dine spil" : profile.crownUnlocked ? "Din stjernekrone er låst op!" : "Vind en stjernekrone", 17));
            words.Add(Text(!playing ? "Saml forskellige belønninger til katten i hvert spil."
                : profile.crownUnlocked ? "Din belønning er din at beholde." : "Kom over 5.000 point i én runde.", 12));
            if (playing && profile.crownUnlocked)
                words.Add(Action(profile.crownEquipped ? "Tag kronen af" : "Brug kronen", () => { profile.crownEquipped = !profile.crownEquipped; Save(); Render(); }, true));
            row.Add(words); box.Add(row); content.Add(box);
        }

        void BuildGame()
        {
            var back = Action("← Alle spil", OpenMenu, true); back.SetEnabled(!busy); content.Add(back);
            content.Add(Text("Cyklusmatch", 36));
            var box = Box("#fffdfa", 12);
            var heading = Row(); var title = Text("En blød begyndelse", 19); title.style.flexGrow = 1; heading.Add(title);
            var restart = Action("↻", () => ShowDialog("Begynd på ny?", "Runden starter forfra. Din rekord og dine belønninger beholdes.", "Start forfra", NewRound), true);
            restart.SetEnabled(!busy); heading.Add(restart); box.Add(heading);
            var stats = Row();
            var points = Text("POINT\n" + Number(game.Score), 24); points.style.flexGrow = 1;
            stats.Add(points); stats.Add(Text("TRÆK TILBAGE\n" + game.Moves, 24, "#b26b80")); box.Add(stats);
            var progress = new VisualElement(); progress.style.height = 5; progress.style.backgroundColor = Hex("#f0e4e7");
            var fill = new VisualElement(); fill.style.height = 5; fill.style.width = Length.Percent((28 - game.Moves) / 28f * 100); fill.style.backgroundColor = Hex("#b76e85"); progress.Add(fill); box.Add(progress);
            box.Add(Text("DIN REKORD  " + Number(profile.bestScore), 11));
            var board = new VisualElement(); board.style.backgroundColor = Hex("#f2e8e8"); board.style.paddingTop = board.style.paddingBottom = 3; Round(board, 12);
            for (int r = 0; r < 7; r++)
            {
                var row = Row();
                for (int c = 0; c < 7; c++)
                {
                    int index = r * 7 + c, value = game.Board[index];
                    var tile = new VisualElement { focusable = true, tabIndex = 0, tooltip = TileNames[value] + ", række " + (r + 1) + ", kolonne " + (c + 1) };
                    tile.style.width = Length.Percent(100f / 7); tile.style.height = 44;
                    tile.style.paddingTop = tile.style.paddingBottom = tile.style.paddingLeft = tile.style.paddingRight = 2;
                    var face = new VisualElement(); face.style.flexGrow = 1; face.style.alignItems = Align.Center; face.style.justifyContent = Justify.Center;
                    face.style.backgroundColor = Hex(TileColors[value]); Round(face, 8);
                    if (selected == index)
                    {
                        face.style.borderTopWidth = face.style.borderBottomWidth = face.style.borderLeftWidth = face.style.borderRightWidth = 2;
                        face.style.borderTopColor = face.style.borderBottomColor = face.style.borderLeftColor = face.style.borderRightColor = Hex("#914361");
                    }
                    face.Add(Icon(TileIcons[value], 31, 34)); tile.Add(face);
                    if (game.Moves == 0) tile.style.opacity = .45f;
                    Vector2 start = Vector2.zero; bool pressed = false;
                    tile.RegisterCallback<PointerDownEvent>(evt => { if (busy || game.Moves == 0) return; start = new Vector2(evt.position.x, evt.position.y); pressed = true; tile.CapturePointer(evt.pointerId); evt.StopPropagation(); });
                    tile.RegisterCallback<PointerCancelEvent>(evt => { pressed = false; if (tile.HasPointerCapture(evt.pointerId)) tile.ReleasePointer(evt.pointerId); });
                    tile.RegisterCallback<PointerUpEvent>(evt =>
                    {
                        if (!pressed) return; pressed = false;
                        if (tile.HasPointerCapture(evt.pointerId)) tile.ReleasePointer(evt.pointerId);
                        Vector2 delta = new Vector2(evt.position.x, evt.position.y) - start;
                        if (Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y)) < 18) Choose(index);
                        else
                        {
                            int target = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? index + (delta.x > 0 ? 1 : -1) : index + (delta.y > 0 ? 7 : -7);
                            if (MatchGame.Adjacent(index, target)) { selected = index; Choose(target); }
                        }
                        evt.StopPropagation();
                    });
                    tile.RegisterCallback<NavigationSubmitEvent>(evt => { Choose(index); evt.StopPropagation(); });
                    row.Add(tile);
                }
                board.Add(row);
            }
            box.Add(board); box.Add(Text(message, 12)); content.Add(box);
        }

        void Choose(int index)
        {
            if (busy || game.Moves == 0) return;
            if (selected == index) { selected = -1; Render(); return; }
            if (selected < 0) { selected = index; message = "Vælg en nabo over, under eller ved siden af."; Render(); return; }
            if (!MatchGame.Adjacent(selected, index)) { message = "Brikkerne skal være naboer. Det første felt er stadig valgt."; Render(); return; }
            int first = selected; selected = -1; StartCoroutine(PlayTurn(first, index));
        }

        IEnumerator PlayTurn(int first, int second)
        {
            busy = true;
            var step = game.Swap(first, second);
            int earned = 0, stars = 0, chain = 0;
            bool missed = step.Cells.Length == 0, jackpot = step.Jackpot;
            message = missed ? "−20 point. Intet match – brikkerne bliver på deres nye pladser." : "Flot match!";
            Render();
            while (step.Cells.Length > 0)
            {
                chain++;
                yield return new WaitForSeconds(.2f);
                earned += step.Points * chain;
                stars += game.Resolve(step, chain, chain == 1 ? new[] { second, first } : Array.Empty<int>());
                Render();
                yield return new WaitForSeconds(.2f);
                step = game.Analyze(); // All jokers are passive during cascades.
            }
            bool unlocked = profile.Record(game.Score); Save(); NextTip(); busy = false;
            if (!missed) message = "+" + Number(earned) + " point" + (jackpot ? " · STJERNEJACKPOT!" : stars > 0 ? " · Ny joker!" : chain > 1 ? " · Matches i kæde!" : " · Flot match!");
            if (unlocked) message += " Du har vundet en stjernekrone! Tryk på den på slutskærmen.";
            Render();
            if (game.Moves == 0) ShowEnding();
        }

        void OpenMenu() { if (busy) return; CloseDialog(); playing = false; selected = -1; Render(); scroll.scrollOffset = Vector2.zero; }
        void NewRound() { CloseDialog(); game = new MatchGame(); playing = true; selected = -1; message = "Byt to nabobrikker. Lav mindst 3 ens på række."; Render(); scroll.scrollOffset = Vector2.zero; }
        void CloseDialog() { if (overlay != null) { overlay.RemoveFromHierarchy(); overlay = null; } }

        VisualElement ShowDialog(string title, string description, string action, System.Action callback)
        {
            CloseDialog(); overlay = new VisualElement(); overlay.style.position = Position.Absolute;
            overlay.style.left = overlay.style.right = overlay.style.top = overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(.28f, .15f, .22f, .55f);
            overlay.style.alignItems = Align.Center; overlay.style.justifyContent = Justify.Center; root.Add(overlay);
            var popup = Box("#fffaf7", 23); popup.style.width = Length.Percent(91); popup.style.maxHeight = Length.Percent(92);
            var body = new ScrollView(ScrollViewMode.Vertical); popup.Add(body);
            var close = Action("×", CloseDialog, true); close.style.alignSelf = Align.FlexEnd; close.style.width = 44; body.Add(close);
            body.Add(Text(title, 25)); body.Add(Text(description, 14));
            var extras = new VisualElement(); body.Add(extras);
            body.Add(Action(action, () => { CloseDialog(); callback(); }));
            overlay.Add(popup); close.Focus(); return extras;
        }

        void ShowEnding()
        {
            var extras = ShowDialog("Din runde er fuldendt!", "Du fik " + Number(game.Score) + " point på 28 træk. Din rekord er " + Number(profile.bestScore) + " point.", "Spil igen", NewRound);
            if (profile.crownUnlocked)
            {
                var caption = Text("Tryk på kronen og kald din kat ind!", 13, "#a17b3d"); extras.Add(caption);
                var stage = new VisualElement(); stage.style.height = 185; stage.style.alignSelf = Align.Center; stage.style.width = 235; stage.style.overflow = Overflow.Hidden;
                var cat = Icon("cat", 118, 140); cat.style.position = Position.Absolute; cat.style.left = -145; cat.style.bottom = 0; cat.style.opacity = 0; stage.Add(cat);
                Button claim = null;
                claim = new Button(() =>
                {
                    claim.SetEnabled(false); profile.crownEquipped = true; Save(); Render();
                    caption.text = "Mjav! Stjernekronen er din kats.";
                    StartCoroutine(CrownAnimation(stage, cat, claim));
                });
                claim.tooltip = "Giv din kat stjernekronen"; claim.style.position = Position.Absolute; claim.style.top = 4; claim.style.left = 75;
                claim.style.width = 85; claim.style.height = 60; claim.style.backgroundColor = Color.clear;
                claim.style.borderTopWidth = claim.style.borderBottomWidth = claim.style.borderLeftWidth = claim.style.borderRightWidth = 0;
                claim.Add(Icon("crown", 72, 49)); stage.Add(claim); extras.Add(stage);
            }
            extras.Add(Action("Til spilmenuen", OpenMenu, true));
        }

        IEnumerator CrownAnimation(VisualElement stage, VisualElement cat, VisualElement crown)
        {
            float elapsed = 0;
            while (elapsed < 1.8f && stage.panel != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float move = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / 1.2f));
                cat.style.opacity = Mathf.Clamp01(elapsed * 5); cat.style.left = Mathf.Lerp(-145, 58, move);
                crown.style.top = Mathf.Lerp(4, 23, Mathf.SmoothStep(0, 1, Mathf.Clamp01((elapsed - .9f) / .9f)));
                yield return null;
            }
        }

        void ShowHelp()
        {
            ShowDialog("Sådan finder du din rytme", "Byt to nabobrikker med tryk eller swipe. Alle byt bruger ét af dine 28 træk. Intet match giver −20 point, men brikkerne bliver flyttet.\n\n3 ens giver 60 point, 4 giver 120, og 5 giver 300 og en joker. L- og T-former tæller som fem, hvis de har fem fælles matchende brikker.\n\nDit eget byt kan aktivere en joker, også ved at flytte et symbol ind til den. Jokere er passive i kæder. Fire med joker giver 240 point, fem giver 600 og en ny joker. Tre stjerner på række giver 3.000 jackpotpoint.\n\nSaml flest mulige point på 28 træk. Cykluslængder varierer; 28 er spillets tema. Over 5.000 point låser en krone til katten op.", "Jeg er klar", () => { });
        }

        void OnDestroy() { if (panel != null) Destroy(panel); }
    }
}
