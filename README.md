# HandyConnect

Plateforme tunisienne qui relie clients et artisans (24 gouvernorats, FR / AR).

```
handyconnect/
├─ frontend/                 HTML + CSS + JavaScript (modules ES, sans build)
│  ├─ *.html                 8 pages (index, artisans, profile, about, contact, support, signin, signup)
│  ├─ css/                   style.css (site) · assistant.css (widget IA)
│  ├─ img/                   logo.svg, logo-white.svg  (placeholders: remplacez par vos logos)
│  └─ js/
│     ├─ main.js             point d'entrée : layout + chargement de la page
│     ├─ api.js · session.js client HTTP, JWT
│     ├─ i18n.js · content.js  FR/AR et contenus éditoriaux
│     ├─ layout.js · components.js · ui.js · icons.js · forms.js
│     ├─ assistant.js        widget de l'assistant IA
│     └─ pages/              un module par page
├─ backend/HandyConnect.Api/ API ASP.NET Core 8 (C#) + Dapper + MySQL
│  ├─ Controllers/           Auth, Artisans, PublicContent, Assistant, Admin
│  ├─ Data/                  repositories SQL + DbSeeder (données de démo)
│  ├─ Services/              TokenService (JWT), AssistantService (IA)
│  ├─ Models/ Options/ Common/
│  └─ Program.cs             DI, JWT, rate limiting, fichiers statiques
├─ database/                 01-schema.sql · 02-reference-data.sql
└─ docker-compose.yml        MySQL 8.4 pour le développement
```

## Lancer en local

Prérequis : .NET 8 SDK, Docker.

```bash
docker compose up -d                       # MySQL + schéma + données de référence
cd backend/HandyConnect.Api
dotnet run --environment Development       # http://localhost:5000 (voir la console)
```

L'API sert aussi le frontend : ouvrez l'URL affichée. Swagger : `/swagger`.
En Development, des données de démo sont créées au 1er démarrage
(`Seed:Demo`) : mot de passe **Demo1234!** pour `client1@example.tn`… `artisan1@example.tn`…,
admin : `admin@handyconnect.tn` / `Admin1234!`.

Sans Docker : exécutez les deux fichiers SQL sur votre MySQL 8 et adaptez
`ConnectionStrings:Default`.

## Assistant IA

`POST /api/assistant/chat` (RAG) : le serveur détecte métier/ville, lit les artisans
et la FAQ en base, puis appelle l'API Anthropic (Claude) avec ce contexte.
La clé n'est **jamais** dans le navigateur :

```bash
dotnet user-secrets init
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
# ou variable d'environnement  Anthropic__ApiKey
```

Sans clé (ou en cas d'erreur réseau), un mode hors-ligne répond à partir des mêmes
données (FAQ + annuaire). Modèle configurable : `Anthropic:Model`.
Garde-fous : limite de 20 req/min/IP, entrée bornée, liens de réponse filtrés côté client,
numéros de téléphone jamais transmis à l'IA.

## Règles métier implémentées

- Profil artisan visible seulement si `subscription_ends_at` est dans le futur
  (essai de `Subscription:TrialDays` jours à l'inscription ; le paiement n'est pas implémenté).
- Numéro de téléphone révélé uniquement aux utilisateurs connectés ; le contact est journalisé.
- Un client ne peut noter qu'un artisan qu'il a contacté (1 avis par couple) ;
  l'avis reste `pending` jusqu'à modération (`GET/PATCH /api/admin/reviews`, rôle admin).
- Mots de passe hachés avec bcrypt, JWT, rate limiting sur login/inscription/formulaires.

## Production

Définissez `Jwt__Key` (≥ 32 caractères aléatoires), `ConnectionStrings__Default`,
`Anthropic__ApiKey`, et laissez `Seed:Demo` à `false`. Servez en HTTPS.

## Ce qui a été nettoyé par rapport à l'ancienne version

Données factices en dur (15 artisans, avis, témoignages, demandes, blog, prix inventés,
statistiques « 15+ », jalons non vérifiables, bureaux/carte fictifs), sections dupliquées,
animations décoratives (orbite, défilement d'avis), liens morts, CSS/JS minifiés illisibles
(classes renommées de façon explicite), formulaires qui ne faisaient rien (désormais branchés à l'API).
