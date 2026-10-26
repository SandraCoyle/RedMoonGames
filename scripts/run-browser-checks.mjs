import {spawnSync} from 'node:child_process';
for(const file of ['menu','touch','stars','jackpot','regressions','round','profile-browser','cat-reward']){
 const result=spawnSync(process.execPath,[`tests/${file}.mjs`],{stdio:'inherit'});
 if(result.status!==0)process.exit(result.status||1);
}
