/* FORMS: one place for every form. Each <form data-form="x"> runs ACT.x.
   API='' = demo mode (nothing sent). Set API='https://your-server/' to POST JSON to API + form action. */
const API='';
const NL=()=>`<div class="w nli"><div class="pn">${L('Restez informé : inscrivez-vous à notre newsletter !','ابق على اطلاع: اشترك في نشرتنا!')}</div><form action="api/newsletter" method="post" data-form="newsletter"><label for="nl-n">${L('Nom','الاسم')}*</label><input id="nl-n" name="name" required minlength="2" maxlength="60" autocomplete="name"><label for="nl-e">Email*</label><input id="nl-e" name="email" type="email" required maxlength="80" autocomplete="email"><button class="bf">${L('S’abonner','اشترك')}</button></form></div>`;
let rs=0;const setR=n=>{rs=n;$$('.rp b').forEach((b,i)=>b.classList.toggle('on',i<n));const h=$('#rt');if(h)h.value=n};
function rform(p){const u=U();$('#rvf').innerHTML=`<h2 class="st">${L('Laisser un <span>avis</span>','اترك <span>تقييمك</span>')}</h2>`+(u?`<form class="fm" style="margin:auto" action="api/review" method="post" data-form="review" data-id="${p.id}"><p class="mu">${L('Votre avis après l’intervention de','رأيك بعد تدخل')} <b>${p.n}</b></p><label>${L('Votre note','تقييمك')} <em>*</em></label><div class="rp">${[1,2,3,4,5].map(i=>`<b onclick="setR(${i})">★</b>`).join('')}</div><input type="hidden" id="rt" name="rating"><label for="rsv">${L('Service réalisé','الخدمة المنجزة')}</label><select id="rsv" name="service">${p.s.map(x=>`<option>${x}</option>`).join('')}</select><label for="rnm">${L('Votre nom','اسمك')}</label><input id="rnm" name="author" value="${E(u)}" required minlength="2" maxlength="60"><label for="rtx">${L('Votre avis','رأيك')} <em>*</em></label><textarea id="rtx" name="comment" minlength="10" maxlength="400" required placeholder="${L('Ponctualité, qualité, prix…','الالتزام بالمواعيد، الجودة، السعر…')}"></textarea><label style="display:flex;gap:8px;align-items:center"><input type="checkbox" name="confirm" required style="width:auto"> ${L('Je confirme que les travaux sont terminés','أؤكد أن الأشغال قد انتهت')}</label><button class="bf">${L('Publier l’avis','نشر التقييم')}</button></form>`:`<p class="c">${L('Connectez-vous pour laisser un avis après une intervention.','سجّل الدخول لترك تقييم بعد التدخل.')}<br><br><a class="bf" href="signin.html">${L('Connexion','دخول')}</a></p>`);setR(0)}

const err=()=>toast(L('Erreur, réessayez.','خطأ، أعد المحاولة.'));
async function send(f){if(!API)return true;try{return(await fetch(API+f.getAttribute('action'),{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(Object.fromEntries(new FormData(f)))})).ok}catch(e){return false}}
const login=(f,n)=>{try{localStorage.hcu=n.trim().split('@')[0]}catch(e){}};
const mail=async f=>{if(!await send(f))return err();f.reset();toast()};
const ACT={
 contact:mail,support:mail,newsletter:mail,
 signin:async f=>{if(!await send(f))return err();login(f,f.elements.login.value);location='index.html'},
 signup:async f=>{if(!await send(f))return err();login(f,f.elements.fullname.value);location='signin.html'},
 filter:()=>{F.k=$('#k').value;F.v=$('#kv').value;R()},
 help:f=>{const q=f.elements.q.value.trim().toLowerCase();$$('#faq details').forEach(d=>d.style.display=!q||d.textContent.toLowerCase().includes(q)?'':'none');location.hash='faq'},
 review:f=>{const e=f.elements,t=e.comment.value.trim();
  if(!rs)return toast(L('Choisissez une note ★','اختر تقييما ★'));
  if(t.length<10)return toast(L('Avis trop court (10 caractères min.)','التقييم قصير جدا'));
  const id=+f.dataset.id,l=RK(id);l.push([E(e.author.value.trim()),L('Client · ','حريف · ')+E(e.service.value),rs,E(t)]);
  try{localStorage['rv'+id]=JSON.stringify(l)}catch(x){}
  toast(L('Merci ! Votre avis est publié ✓','شكرا! تم نشر تقييمك ✓'));setTimeout(()=>location.reload(),900)}
};
document.addEventListener('submit',e=>{const a=ACT[e.target.dataset.form];if(a){e.preventDefault();a(e.target)}});
/* signup: artisan fields only when role=artisan (disabled fieldset = not validated, not sent) */
const role=()=>{const f=$('#m'),a=$('#ra');if(f&&a)f.hidden=f.disabled=!a.checked};
document.addEventListener('change',e=>{if(e.target.name=='role')role()});
document.addEventListener('input',e=>{if(e.target.id=='su-p'||e.target.id=='su-c'){const c=$('#su-c');c.setCustomValidity(c.value&&c.value!=$('#su-p').value?L('Les mots de passe diffèrent','كلمتا المرور مختلفتان'):'')}});
document.addEventListener('click',e=>{const b=e.target.closest('.pe');if(b){const i=b.previousElementSibling;i.type=i.type=='password'?'text':'password'}});
document.addEventListener('DOMContentLoaded',()=>{if(/[?&]r=a/.test(location.search)&&$('#ra'))$('#ra').checked=true;role()});
