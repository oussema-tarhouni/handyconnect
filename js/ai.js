/* Assistant HandyConnect. Local mode works offline (answers from site data).
   To use a real AI: set AI_ENDPOINT to your backend URL that accepts POST {message,lang} and returns {reply}. */
const AI_ENDPOINT='';
(()=>{
IP.sparkles="M12 3l1.9 5.1L19 10l-5.1 1.9L12 17l-1.9-5.1L5 10l5.1-1.9L12 3zM19 15l.9 2.1L22 18l-2.1.9L19 21l-.9-2.1L16 18l2.1-.9L19 15z";
const A=s=>s.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g,'');
const lk=(h,t)=>`<a href="${h}">${t}</a>`;
const KB=[
[/bonjour|salut|hello|salam|مرحبا|سلام|اهلا/,['Bonjour ! Je peux vous aider à trouver un artisan, comprendre les prix, les avis ou l’inscription.','مرحبا! يمكنني مساعدتك في إيجاد حرفي أو فهم الأسعار والتقييمات والتسجيل.']],
[/gratuit|prix|tarif|cout|payer|paiement|abonnement|مجاني|سعر|دفع|اشتراك|تكلفة/,['C’est gratuit pour les clients. Les artisans souscrivent un abonnement (exemple affiché : 29 DT/mois). Pas de paiement en ligne : le prix se fixe avec l’artisan avant les travaux.','مجاني للحرفاء. الحرفيون يشتركون باشتراك شهري. لا دفع إلكتروني: السعر يُتفق عليه مع الحرفي قبل الأشغال.']],
[/avis|note|etoile|commentaire|تقييم|رأي|نجوم|تعليق/,['Après une intervention, vous notez de 1 à 5 étoiles et commentez. Chaque avis est lié à une intervention réelle et modéré par l’administration. '+lk('signin.html','Se connecter'),'بعد التدخل تقيّم من 1 إلى 5 نجوم وتعلّق. كل تقييم مرتبط بتدخل حقيقي وتراجعه الإدارة. '+lk('signin.html','دخول')]],
[/devenir|artisan.*(inscri|profil)|inscri|compte|creer|حساب|تسجيل|انضم|كن حرفي/,['Créez un compte client (gratuit) ou artisan (métier, spécialités, zone + abonnement). '+lk('signup.html','S’inscrire')+' · '+lk('signup.html?r=a','Devenir artisan'),'أنشئ حساب حريف (مجاني) أو حرفي (مهنة، اختصاصات، منطقة + اشتراك). '+lk('signup.html','تسجيل')+' · '+lk('signup.html?r=a','كن حرفيا')]],
[/connexion|connecter|mot de passe|login|دخول|كلمة السر/,['Connectez-vous pour voir les numéros et laisser un avis : '+lk('signin.html','Connexion'),'سجّل الدخول لرؤية الأرقام وترك تقييم: '+lk('signin.html','دخول')]],
[/numero|appeler|appel|telephone|joindre|رقم|اتصال|هاتف/,['Ouvrez la fiche de l’artisan puis appelez-le directement après connexion. Aucun intermédiaire ni commission.','افتح ملف الحرفي ثم اتصل به مباشرة بعد تسجيل الدخول. دون وسيط أو عمولة.']],
[/disparait|disparu|masque|visible|يختفي|يظهر/,['Le profil d’un artisan est masqué automatiquement quand son abonnement expire.','يُخفى ملف الحرفي تلقائيا عند انتهاء اشتراكه.']],
[/langue|arabe|francais|لغة|عربي|فرنسي/,['Utilisez le bouton FR / ع en haut pour changer de langue (le site passe en arabe de droite à gauche).','استعمل زر FR / ع في الأعلى لتغيير اللغة.']],
[/comment|marche|fonctionne|كيف|طريقة/,['1) Cherchez un métier et un gouvernorat 2) Consultez fiches et avis 3) Appelez l’artisan 4) Évaluez après l’intervention.','1) ابحث عن مهنة وولاية 2) اطلع على الملفات والتقييمات 3) اتصل بالحرفي 4) قيّم بعد التدخل.']],
[/contact|support|aide|probleme|bug|اتصل|مساعدة|مشكل/,['Notre équipe répond sous 24 h : '+lk('contact.html','Contact')+' · '+lk('support.html','Centre d’aide'),'فريقنا يرد خلال 24 ساعة: '+lk('contact.html','اتصل بنا')+' · '+lk('support.html','مركز المساعدة')]]
];
const best=()=>[...P].sort((a,b)=>b.r-a.r).slice(0,3);
const lst=a=>a.map(p=>`<br>• ${lk('profile.html?id='+p.id,p.n)} — ${jn(p.j)}, ${p.v} ★${p.r}`).join('');
function bot(q){
 const a=A(q),c=C.find(c=>a.includes(A(c[0]).slice(0,5))||q.includes(c[1])),g=G.find(g=>a.includes(A(g)));
 if(c||g){let r=P.filter(p=>(!c||p.j==c[0])&&(!g||p.v==g)).sort((x,y)=>y.r-x.r);let n='';
  if(!r.length&&c){r=P.filter(p=>p.j==c[0]).sort((x,y)=>y.r-x.r);n=L(`Aucun à ${g} pour l’instant, voici les meilleurs ${c[0]}s : `,'لا يوجد هناك حاليا، هذه أفضل النتائج: ')}
  else if(!r.length)return L('Aucun artisan dans cette ville pour l’instant. ','لا يوجد حرفيون هناك حاليا. ')+lk('artisans.html','Annuaire');
  return n+L(`${r.length} artisan(s) trouvé(s) :`,`${r.length} حرفي:`)+lst(r.slice(0,3))+`<br>${lk(`artisans.html?m=${c?c[0]:''}&v=${g||''}`,L('Voir tout →','عرض الكل ←'))}`}
 if(/meilleur|mieux|top|recommand|الأفضل|افضل/.test(a+q))return L('Les artisans les mieux notés :','الأفضل تقييما:')+lst(best());
 for(const[r,t]of KB)if(r.test(a)||r.test(q))return L(t[0],t[1]);
 return L('Je n’ai pas bien compris. Essayez par exemple « plombier à Sfax », « est-ce gratuit ? » ou ','لم أفهم جيدا. جرّب مثلا «سباك في صفاقس» أو «هل هو مجاني؟» أو ')+lk('support.html',L('le Centre d’aide','مركز المساعدة'));
}
async function ask(q){if(AI_ENDPOINT){try{const r=await fetch(AI_ENDPOINT,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({message:q,lang:LG})});const j=await r.json();if(j.reply)return j.reply.replace(/</g,'&lt;')}catch(e){}}return bot(q)}
const SUG=[['Plombier à Tunis','سباك في تونس'],['Est-ce gratuit ?','هل هو مجاني؟'],['Comment laisser un avis ?','كيف أترك تقييما؟'],['Meilleurs artisans','أفضل الحرفيين']];
const CSS=`@keyframes hcr{0%{box-shadow:0 0 0 0 rgba(61,90,241,.5)}100%{box-shadow:0 0 0 16px rgba(61,90,241,0)}}@keyframes hbob{50%{transform:translateY(-6px)}}@keyframes hrp{0%{transform:scale(1);opacity:.75}100%{transform:scale(1.45,1.7);opacity:0}}@keyframes hdt{50%{transform:translateY(-4px);opacity:.4}}@keyframes hfu{from{opacity:0;transform:translateY(10px)}to{opacity:1;transform:none}}@keyframes tw{0%,100%{transform:scale(.55) rotate(0);opacity:.55}50%{transform:scale(1.15) rotate(20deg);opacity:1}}
.hcb{position:fixed;top:90%;right:45px;margin-top:-30px;z-index:9999;height:60px;padding:0 14px 0 12px;border:0;border-radius:30px;background:#fff;box-shadow:0 6px 22px rgba(20,22,58,.25);cursor:pointer;display:flex;align-items:center;gap:6px;animation:hbob 3s ease-in-out infinite}.hcb:hover{animation-play-state:paused}.hcb.x{animation:none}.hcb:before,.hcb:after{content:'';position:absolute;inset:0;border-radius:30px;border:2px solid #3D5AF1;pointer-events:none;animation:hrp 2.4s ease-out infinite}.hcb:after{animation-delay:1.2s}.hcb.x:before,.hcb.x:after{display:none}.hcb img{height:38px;width:auto}
.hst{position:relative;width:24px;height:36px}.hs{position:absolute;fill:currentColor;stroke:none}.h1{width:20px;height:20px;top:0;left:0;color:#3D5AF1}.h2{width:12px;height:12px;top:21px;left:12px;color:#3D5AF1}
.hcp{position:fixed;top:auto;bottom:max(16px,calc(10% - 30px));right:150px;z-index:9999;width:380px;max-width:calc(100vw - 170px);height:500px;max-height:80vh;background:#fff;border-radius:12px;box-shadow:0 18px 50px rgba(20,22,58,.22);display:flex;flex-direction:column;overflow:hidden;opacity:0;pointer-events:none;transform:translateX(20px) scale(.96);transform-origin:right bottom;transition:.3s cubic-bezier(.2,.8,.2,1)}.hcp.on{opacity:1;pointer-events:auto;transform:none}.hcp button{font-family:inherit}
.hcp header{background:#F3F4F6;color:#14163A;padding:12px 16px;display:flex;justify-content:space-between;align-items:center;border-bottom:1px solid #E5E7EB}.hcp header b{display:flex;align-items:center;gap:8px}.hcp header button{background:none;border:0;color:#14163A;cursor:pointer;font-size:1rem}
.hct{font:600 1rem Poppins,Cairo,sans-serif}.hct span{color:#3D5AF1}.hcl{height:34px;width:auto}.hs2{width:15px;height:15px;fill:#FFB400;stroke:none}
.hcm{flex:1;overflow:auto;padding:16px;display:flex;flex-direction:column;gap:10px;background:#fff}.hm{max-width:85%;padding:10px 14px;border-radius:8px;font-size:.9rem;animation:hfu .3s both}.hm a{color:#3D5AF1;text-decoration:underline;font-weight:600}
.hm.b{background:#F3F4F6;align-self:flex-start}.hm.u{background:#3D5AF1;color:#fff;align-self:flex-end}
.ty i{display:inline-block;width:7px;height:7px;margin:0 2px;border-radius:50%;background:#6B7080;animation:hdt 1s infinite}.ty i:nth-child(2){animation-delay:.15s}.ty i:nth-child(3){animation-delay:.3s}
.hcc{display:flex;gap:6px;overflow-x:auto;padding:10px 14px;border-top:1px solid #E5E7EB}.hcc button{flex:0 0 auto;border:1px solid #14163A;background:#fff;color:#14163A;border-radius:20px;padding:4px 12px;font-size:.78rem;cursor:pointer;transition:.2s}.hcc button:hover{background:#3D5AF1;border-color:#3D5AF1;color:#fff}
.hcp form{display:flex;gap:8px;padding:10px 14px 14px}.hcp form .bf{padding:0 18px}
@media(max-width:600px){.hcp{bottom:84px;right:16px;left:16px;width:auto;max-width:none;transform:translateY(20px) scale(.96)}.hcp.on{transform:none}}`;
document.addEventListener('DOMContentLoaded',()=>{
 document.head.insertAdjacentHTML('beforeend','<style>'+CSS+'</style>');
 document.body.insertAdjacentHTML('beforeend',`<button class="hcb" id="hcb" aria-label="Assistant"><img src="img/logo.svg" alt=""><span class="hst">${ic('sparkles','hs h1')}${ic('sparkles','hs h2')}</span></button><section class="hcp" id="hcp" aria-live="polite"><header><b id="hch"></b><button id="hcx" aria-label="Fermer">✕</button></header><div class="hcm" id="hcm"></div><div class="hcc" id="hcc"></div><form id="hcf" onsubmit="return false"><input id="hcq" name="q" maxlength="200" autocomplete="off"><button class="bf">➤</button></form></section>`);
 const m=$('#hcm'),add=(h,c)=>{const d=document.createElement('div');d.className='hm '+c;d.innerHTML=h;m.appendChild(d);m.scrollTop=m.scrollHeight;return d};
 const ui=()=>{$('#hch').innerHTML='<img class="hcl" src="img/logo.svg" alt=""><span class="hct">'+L('Assistant <span>HandyConnect</span>','مساعد <span>HandyConnect</span>')+'</span>'+ic('sparkles','hs2');$('#hcq').placeholder=L('Posez votre question…','اطرح سؤالك…');$('#hcc').innerHTML=SUG.map(s=>`<button>${L(s[0],s[1])}</button>`).join('');if(!m.children.length)add(L('Bonjour ! Posez-moi n’importe quelle question sur HandyConnect : artisans, prix, avis, inscription…','مرحبا! اسألني عن أي شيء في HandyConnect: الحرفيون، الأسعار، التقييمات، التسجيل…'),'b')};
 const send=async q=>{q=q.trim();if(!q)return;add('','u').textContent=q;$('#hcq').value='';const t=add('<i></i><i></i><i></i>','b ty');const r=await ask(q);await new Promise(r=>setTimeout(r,500));t.className='hm b';t.innerHTML=r;m.scrollTop=m.scrollHeight};
 $('#hcb').onclick=()=>{const o=$('#hcp').classList.toggle('on');$('#hcb').classList.toggle('x',o);if(o){ui();$('#hcq').focus()}};
 $('#hcx').onclick=()=>{$('#hcp').classList.remove('on');$('#hcb').classList.remove('x')};
 $('#hcf').onsubmit=e=>{e.preventDefault();send($('#hcq').value)};
 $('#hcc').onclick=e=>{if(e.target.tagName=='BUTTON')send(e.target.textContent)};
});
})();
