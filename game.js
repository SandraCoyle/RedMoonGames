import {advanceProfile,loadProfile,saveProfile} from './profile.js';
import {SIZE,createBoard,matches,swap,adjacent,matchScore,resolveMatch,activeStarsForMove,starLines} from './engine.js';
const icons=['🩸','🌙','🌸','','','','⭐'];
const names=['Dråbe','Måne','Blomst','Tampon','Menstruationsbind','Menstruationskop','Stjerne – joker'];
const colors=['#f3d9df','#e6def1','#f6e2da','#dce9ed','#ede4d5','#edd9e4','#ffedb8'];
const facts=[
 "Din første menstruation kan være lidt blod eller en brun plet i trusserne. Begge dele kan være helt almindeligt.",
 "Det er almindeligt, at menstruationen kommer uregelmæssigt i starten. Den behøver ikke komme samme dato hver måned.",
 "Et bind sættes fast i trusserne. Har det vinger, foldes de rundt om kanten, så bindet sidder bedre fast.",
 "Skift bind jævnligt og før det bliver gennemblødt. Følg også vejledningen på pakken.",
 "Vask hænder før og efter, du skifter dit menstruationsprodukt.",
 "En lille taske med et ekstra bind og et par rene trusser kan være rar at have med i skole.",
 "Kommer menstruationen i skolen, kan du spørge en lærer, sundhedsplejersken eller en ven om hjælp.",
 "Bløder du igennem, har du ikke gjort noget forkert. Skift produkt og tøj, når du kan, og bed gerne om hjælp.",
 "Koldt vand kan hjælpe med at skylle friske blodpletter ud af tøjet.",
 "Pak brugte bind ind og læg dem i skraldespanden. Bind og tamponer skal ikke i toilettet.",
 "Et varmt omslag på maven kan føles rart ved menstruationssmerter. Det skal være behageligt varmt, ikke brændende.",
 "Du må gerne hvile dig. En rolig gåtur eller lidt bevægelse kan også føles rart. Mærk efter, hvad du har lyst til.",
 "Du kan godt være med til sport, når du har menstruation. Bed om en pause, hvis du har brug for det.",
 "Du behøver ikke starte med tampon eller kop. Et bind eller menstruationstrusser kan være et fint sted at begynde.",
 "Hvis du vil prøve en tampon, så følg vejledningen på pakken, og lad den aldrig sidde længere end 8 timer.",
 "Det kan tage lidt tid at finde et produkt, der passer dig. Du må gerne prøve dig frem og spørge om hjælp.",
 "Du kan skrive den første blødningsdag i en kalender, hvis det hjælper dig med at lære din menstruation at kende.",
 "Har du så ondt, at du ikke kan passe skole eller gøre de ting, du plejer, så fortæl det til en voksen, du stoler på, og få hjælp hos lægen.",
 "Bløder du igennem et bind eller en tampon på omkring en time, især flere gange i træk, så sig det til en voksen og kontakt lægen.",
 "Du behøver ikke vide det hele fra starten. Spørg en voksen, du stoler på, eller sundhedsplejersken, når du er i tvivl."
];
let factOrder=[],factIndex=-1;
function nextFact(){
 if(factIndex+1>=factOrder.length){const previous=factOrder[factIndex];factOrder=facts.map((_,i)=>i);for(let i=factOrder.length-1;i>0;i--){const j=Math.floor(Math.random()*(i+1));[factOrder[i],factOrder[j]]=[factOrder[j],factOrder[i]];}if(factOrder[0]===previous)[factOrder[0],factOrder[1]]=[factOrder[1],factOrder[0]];factIndex=-1;}
 $('fact').textContent=facts[factOrder[++factIndex]];
}
const $=id=>document.getElementById(id),wait=ms=>new Promise(r=>setTimeout(r,ms));
let storage;try{storage=window.localStorage;}catch{}
const loadedProfile=loadProfile(storage);
let profile=loadedProfile.profile,storageAvailable=loadedProfile.available;
let currentView='menu',crownNotice=false;
function persistProfile(){storageAvailable=saveProfile(storage,profile);renderProfile();}
function renderProfile(){
 $('best-score').textContent=profile.bestScore.toLocaleString('da-DK');
 $('avatar').classList.toggle('crowned',profile.crownEquipped);
 $('avatar').setAttribute('aria-label',profile.crownEquipped?'Din kat med stjernekrone':'Din kat uden stjernekrone');
 const inCyklusmatch=currentView==='cyklusmatch';
 $('reward-title').textContent=inCyklusmatch?(profile.crownUnlocked?'Din stjernekrone er låst op!':'Vind en stjernekrone'):'Din kat';
 $('reward-description').textContent=inCyklusmatch?(profile.crownUnlocked?'Din belønning er din at beholde.':'Kom over 5.000 point i én runde, og vind kronen til din avatar.'):'Din kat er med i alle spillene. Saml forskellige belønninger til den i hvert spil.';
 $('equip-crown').hidden=!inCyklusmatch||!profile.crownUnlocked;
 $('reward-progress').hidden=!inCyklusmatch;
 $('reward-notice').hidden=!inCyklusmatch||!crownNotice;
 $('equip-crown').textContent=profile.crownEquipped?'Tag kronen af':'Brug stjernekronen';
 $('equip-crown').setAttribute('aria-pressed',String(profile.crownEquipped));
 $('reward-progress').value=Math.min(profile.bestScore,5001);
 $('reward-progress').setAttribute('aria-label',profile.crownUnlocked?'Stjernekrone låst op':`${profile.bestScore.toLocaleString('da-DK')} af 5.001 point mod stjernekronen`);
 $('save-status').textContent=storageAvailable?'Rekord og belønning gemmes i denne browser.':'Browseren tillader ikke lagring. Rekord og belønning gælder kun, mens spillet er åbent.';
}
function recordProgress(){
 const result=advanceProfile(profile,score);
 if(JSON.stringify(profile)!==JSON.stringify(result.profile)){profile=result.profile;persistProfile();}
 if(result.newlyUnlocked){crownNotice=true;$('reward-notice').hidden=currentView!=='cyklusmatch';$('reward-notice').textContent='✨ Over 5.000 point! Du har vundet en stjernekrone. Tryk på kronen på slutskærmen, og giv den til din kat.';}
}
let board,score,moves,selected=-1,busy=false,ended=false,round=0,action;
const games=[
 {id:'cyklusmatch',title:'Cyklusmatch',description:'Match symboler, skab stjerner, og slå din egen rekord.',tag:'MATCH-3 · 28 TRÆK',available:true},
 {id:'memory',title:'Husk & match',description:'Et lille vendespil med velkendte symboler.',tag:'VENDESPIL',available:false},
 {id:'mooncatch',title:'Månefangst',description:'Fang måner og find din egen rytme.',tag:'REAKTIONSSPIL',available:false}
];
function renderMenu(){
 $('game-list').replaceChildren(...games.map(game=>{
  const card=document.createElement('article');card.className='game-card'+(game.available?' featured':' upcoming');
  const art=document.createElement('div');art.className='game-art';art.setAttribute('aria-hidden','true');
  if(game.available)art.innerHTML='<span>🩸</span><span>🌙</span><span>🌸</span><span><svg viewBox="0 0 64 64"><use href="#pad-icon"></use></svg></span><span>⭐</span><span><svg viewBox="0 0 64 64"><use href="#cup-icon"></use></svg></span>';
  else art.textContent=game.id==='memory'?'✿ ✿':'☾ ✧';
  const details=document.createElement('div');details.className='game-card-info';
  const tag=document.createElement('span');tag.className='eyebrow';tag.textContent=game.tag;
  const title=document.createElement('h3');title.textContent=game.title;
  const description=document.createElement('p');description.textContent=game.description;
  details.append(tag,title,description);
  if(game.available){
   const best=document.createElement('p');best.className='card-record';best.textContent=`Din rekord: ${profile.bestScore.toLocaleString('da-DK')} point`;details.append(best);
   const button=document.createElement('button');button.type='button';button.id='play-'+game.id;button.className='primary';button.textContent=ended?'Spil igen':moves<28?'Fortsæt runden':'Spil Cyklusmatch';button.onclick=()=>{if(ended)reset();navigate('cyklusmatch');};details.append(button);
  }else{const soon=document.createElement('span');soon.className='coming-soon';soon.textContent='Kommer senere';details.append(soon);}
  card.append(art,details);return card;
 }));
}
function navigate(view){
 const playing=view==='cyklusmatch';
 currentView=playing?'cyklusmatch':'menu';renderProfile();
 $('menu-view').hidden=playing;$('game-view').hidden=!playing;$('game-extras').hidden=!playing;
 $('help').hidden=!playing;
 document.title=playing?'Cyklusmatch · Rød Måne':'Spillerummet · Rød Måne';
 if(!playing)renderMenu();
 (playing?$('game-title'):$('menu-title')).focus({preventScroll:true});
 window.scrollTo({top:0,behavior:'instant'});
}
function setupBoard(){
 $('board').replaceChildren(...Array.from({length:SIZE*SIZE},(_,i)=>{
  const b=document.createElement('button');b.type='button';b.className='tile';
  let start=null,suppressClick=false;
  b.onclick=()=>{if(suppressClick){suppressClick=false;return;}choose(i);};
  b.onpointerdown=e=>{if(busy||ended)return;start=[e.clientX,e.clientY];suppressClick=false;b.setPointerCapture(e.pointerId);};
  b.onpointercancel=()=>{start=null;};
  b.onpointerup=e=>{
   if(!start||busy||ended)return;
   const dx=e.clientX-start[0],dy=e.clientY-start[1];start=null;
   if(Math.max(Math.abs(dx),Math.abs(dy))<18)return;
   suppressClick=true;
   const target=Math.abs(dx)>Math.abs(dy)?i+(dx>0?1:-1):i+(dy>0?SIZE:-SIZE);
   if(target>=0&&target<SIZE*SIZE&&adjacent(i,target)){selected=i;choose(target);}
  };
  return b;
 }));
}
function render(){
 if(!$('board').children.length)setupBoard();
 board.forEach((v,i)=>{const b=$('board').children[i];b.className='tile'+(selected===i?' selected':'');b.style.setProperty('--tile',colors[v]);if(v===3||v===4||v===5){const icon=v===3?'tampon-icon':v===4?'pad-icon':'cup-icon';if(b.dataset.type!==String(v))b.innerHTML=`<svg class="pad-icon" aria-hidden="true">${v===3?'<title>Tampon</title>':v===5?'<title>Menstruationskop</title>':''}<use href="#${icon}"></use></svg>`;}else b.textContent=icons[v];b.dataset.type=v;b.setAttribute('aria-label',`${names[v]}, række ${Math.floor(i/SIZE)+1}, kolonne ${i%SIZE+1}`);b.setAttribute('aria-pressed',String(selected===i));b.disabled=busy||ended;});
 $('board').classList.toggle('finished',ended);
 $('score').textContent=score.toLocaleString('da-DK');$('moves').textContent=moves;$('progress').style.width=(28-moves)/28*100+'%';document.querySelector('.progress').setAttribute('aria-valuenow',28-moves);
}
function show(title,text,label,fn,reward=false){$('dialog-menu').hidden=!reward;$('crown-scene').hidden=!reward||!profile.crownUnlocked;$('crown-scene').classList.remove('claimed');$('claim-crown').disabled=false;$('claim-crown').setAttribute('aria-label','Giv din kat stjernekronen');$('reward-cat').setAttribute('aria-hidden','true');$('crown-instruction').textContent='Tryk på kronen og kald din kat ind!';$('dialog-title').textContent=title;$('dialog-text').textContent=text;$('dialog-action').textContent=label;action=fn;$('dialog').showModal();}
function reset(){round++;board=createBoard();score=0;moves=28;selected=-1;busy=false;ended=false;$('status').textContent='Tryk på et felt og derefter en nabo. Lav 3 ens på række.';$('move-result').textContent='Match 3+ på række = plus · Intet match = −20 point';$('move-result').className='move-result';nextFact();render();}
async function choose(i){
 if(busy||ended)return;
 if(selected===i){selected=-1;$('status').textContent='Vælg et felt, og derefter feltet lige ved siden af.';render();return;}
 if(selected<0){selected=i;$('status').textContent='Felt valgt. Tryk lige over, under eller ved siden af for at bytte.';render();return;}
 if(!adjacent(selected,i)){$('status').textContent='De to felter skal være naboer. Det første felt er stadig valgt.';return;}
 const a=selected,token=round;selected=-1;busy=true;swap(board,a,i);let activeStars=activeStarsForMove(board,[a,i]);render();let hit=matches(board,activeStars);
 moves--;let combo=0,earned=0,starsEarned=0;
 const missed=!hit.length;const jackpot=starLines(board,activeStars).length>0;
 if(missed){$('move-result').textContent='−20 point · Byttet lavede ikke 3 ens vandret eller lodret.';$('move-result').className='move-result negative';score-=20;render();await wait(180);if(token!==round)return;}
 while(hit.length){combo++;for(const k of hit)$('board').children[k].classList.add('popping');await wait(220);if(token!==round)return;const points=matchScore(board,combo,activeStars);score+=points;earned+=points;starsEarned+=resolveMatch(board,combo===1?[i,a]:[],Math.random,activeStars);render();await wait(180);if(token!==round)return;activeStars=[];hit=matches(board);}
 if(!missed){$('move-result').textContent=`+${earned} point · ${jackpot?'⭐ STJERNEJACKPOT!':starsEarned?`Du fik ${starsEarned} ⭐ joker!`:combo>1?'Matches i kæde!':'3 eller flere ens på række!'}`;$('move-result').className='move-result positive';}
 recordProgress();
 nextFact();
 if(moves===0){ended=true;busy=false;render();$('status').textContent='Alle 28 træk er brugt. Godt spillet! ✨';show('Din runde er fuldendt!',`Du fik ${score.toLocaleString('da-DK')} point på 28 træk. ${profile.crownUnlocked?'Din stjernekrone er låst op! ':''}Din rekord er ${profile.bestScore.toLocaleString('da-DK')} point. Klar til en ny runde?`,'Spil igen',reset,true);return;}
 $('status').textContent=missed?'−20 point: ingen 3 på række. Brikkerne bliver på deres nye pladser.':combo>1?`+${earned} point · ${combo} matches i kæde! ✨`:`+${earned} point · Flot match! 🌙`;busy=false;render();
}
$('restart').onclick=()=>show('Begynd på ny?','Dine point og træk bliver nulstillet, så du kan starte en frisk runde.','Start forfra',reset);
$('help').onclick=()=>show('Sådan finder du din rytme','Tryk på to nabofelter, eller swipe et felt, for at bytte dem. Match mindst 3 ens vandret eller lodret. 3 på række giver 60 point, 4 giver 120, og 5 giver 300. Længere rækker giver 20 pr. felt plus 200 i bonus. 5 eller flere sammenhængende matchende brikker giver en ⭐ joker, også i L- og T-former og med en joker. To rækker på 3, som deler en brik, tæller som 5. Den kan erstatte alle symboler. 4 på række med en stjerne giver 240 point; 5 giver 600. Jokeren aktiveres, når dit eget byt laver et match med den – også når du flytter et andet symbol ind til den. Den bruges aldrig automatisk i kædereaktioner. Tre stjerner på række giver 3.000 jackpotpoint, plus 1.000 for hver ekstra stjerne i rækken. Det skal laves med dit eget byt, aldrig i en automatisk kæde. Flere rækker tælles sammen, og kæder ganger pointene med kædens nummer. Saml så mange point som muligt på 28 træk. Runden slutter først, når alle træk er brugt. De 28 træk er inspireret af cyklussen; cykluslængder varierer. Alle byt bruger ét træk. Byt uden match giver −20 point, og brikkerne bliver på deres nye pladser. Din score kan gå under nul.','Jeg er klar',()=>{});
$('back-menu').onclick=()=>navigate('menu');
$('home-link').onclick=e=>{e.preventDefault();if($('dialog').open)$('dialog').close();navigate('menu');};
$('dialog-menu').onclick=()=>{$('dialog').close();navigate('menu');};
$('claim-crown').onclick=()=>{if(!profile.crownUnlocked)return;profile={...profile,crownEquipped:true};persistProfile();$('claim-crown').disabled=true;$('claim-crown').setAttribute('aria-label','Din kat har fået kronen på');$('reward-cat').setAttribute('aria-hidden','false');$('crown-scene').classList.add('claimed');$('crown-instruction').textContent='Mjav! Stjernekronen er din kats 👑';};
$('equip-crown').onclick=()=>{profile={...profile,crownEquipped:!profile.crownEquipped};persistProfile();};
renderProfile();
$('next-fact').onclick=nextFact;
$('close').onclick=()=>$('dialog').close();$('dialog-action').onclick=()=>{$('dialog').close();action?.();};reset();navigate('menu');
