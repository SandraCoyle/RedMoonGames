export const PROFILE_KEY='cyklusmatch.profile.v1';
export function normalizeProfile(value){
 const bestScore=Number.isSafeInteger(value?.bestScore)&&value.bestScore>0?value.bestScore:0;
 const crownUnlocked=value?.crownUnlocked===true||bestScore>5000;
 return {bestScore,crownUnlocked,crownEquipped:crownUnlocked&&value?.crownEquipped===true};
}
export function advanceProfile(profile,score){
 const bestScore=Math.max(profile.bestScore,Number.isSafeInteger(score)?score:0);
 const newlyUnlocked=!profile.crownUnlocked&&bestScore>5000;
 return {profile:{bestScore,crownUnlocked:profile.crownUnlocked||newlyUnlocked,crownEquipped:profile.crownEquipped},newlyUnlocked};
}
export function loadProfile(storage){
 try{return {profile:normalizeProfile(JSON.parse(storage.getItem(PROFILE_KEY))),available:true};}
 catch{return {profile:normalizeProfile(null),available:false};}
}
export function saveProfile(storage,profile){try{storage.setItem(PROFILE_KEY,JSON.stringify(profile));return true;}catch{return false;}}
