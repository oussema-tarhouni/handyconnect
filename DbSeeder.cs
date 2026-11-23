using Dapper;

namespace HandyConnect.Api.Data;

/// <summary>
/// Optional demo data (Seed:Demo = true, enabled by default only in Development):
/// an admin, 8 demo clients, 15 artisans and a few approved reviews. Runs only on an empty users table.
/// Demo password for every account: Demo1234!  (admin: Admin1234!)
/// </summary>
public sealed class DbSeeder(IDbConnectionFactory factory, ILogger<DbSeeder> logger)
{
    private sealed class IdName
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed record SeedArtisan(string Name, string Trade, string Governorate, int Years, string Bio, string[] Specialties, string Zone);

    private static readonly string[] ClientNames =
    {
        "Imen Kallel", "Mehdi Slim", "Salma Bouaziz", "Omar Tlili",
        "Nour Hamdi", "Rached Mansour", "Yosra Ayadi", "Aymen Dhaoui"
    };

    private static readonly string[] Comments =
    {
        "Travail soigné, prix annoncé avant les travaux. Je recommande !",
        "Intervention rapide et propre, problème réglé en une heure.",
        "Très bon travail, finitions soignées et délais respectés.",
        "Il a expliqué chaque étape. Confiance totale.",
        "J’ai comparé plusieurs artisans grâce aux avis. Choix facile.",
        "Sérieux et bon rapport qualité-prix. Je le rappellerai.",
        "Dépannage en urgence un dimanche, merci HandyConnect !",
        "Appel direct, prix clair, aucun frais caché. Parfait."
    };

    // Trade slugs match database/02-reference-data.sql.
    private static readonly SeedArtisan[] Artisans =
    {
        new("Ahmed Ben Salah", "plombier", "Tunis", 8, "Fuites, chauffe-eau et installations sanitaires.", new[] { "Fuites", "Chauffe-eau", "Chauffage" }, "Grand Tunis"),
        new("Sami Trabelsi", "electricien", "Sfax", 10, "Installations électriques et dépannage.", new[] { "Pannes", "Tableau électrique", "Éclairage" }, "Sfax et environs"),
        new("Rania Ayari", "peintre", "Sousse", 6, "Peinture intérieure et décoration.", new[] { "Peinture", "Enduit", "Façade" }, "Sousse, Monastir"),
        new("Karim Jlassi", "mecanicien", "Nabeul", 12, "Diagnostic et entretien automobile.", new[] { "Diagnostic", "Freins", "Vidange" }, "Nabeul, Hammamet"),
        new("Moez Gharbi", "macon", "Ariana", 5, "Rénovation, carrelage et maçonnerie générale.", new[] { "Carrelage", "Murs", "Rénovation" }, "Grand Tunis"),
        new("Hichem Dridi", "macon", "Bizerte", 15, "Dalles, murs porteurs et rénovation.", new[] { "Dalles", "Murs", "Rénovation" }, "Bizerte"),
        new("Anis Bouzid", "electricien", "Tunis", 9, "Domotique, caméras et dépannage urgent.", new[] { "Domotique", "Caméras", "Dépannage" }, "Grand Tunis"),
        new("Walid Hammami", "plombier", "Ariana", 7, "Salle de bain complète et recherche de fuite.", new[] { "Salle de bain", "Robinetterie", "Fuites" }, "Ariana, Tunis"),
        new("Slim Chaabane", "mecanicien", "Sfax", 11, "Moteur, climatisation et électricité auto.", new[] { "Moteur", "Clim auto", "Électricité auto" }, "Sfax"),
        new("Fathi Mejri", "plombier", "Monastir", 6, "Installation de chauffage et chauffe-eau.", new[] { "Chauffage", "Chauffe-eau" }, "Monastir, Sousse"),
        new("Nizar Ouni", "peintre", "Ben Arous", 4, "Peinture d’appartements et de villas.", new[] { "Appartement", "Villa", "Enduit" }, "Grand Tunis"),
        new("Taher Zouari", "electricien", "Kairouan", 13, "Câblage, prises et mise aux normes.", new[] { "Câblage", "Prises", "Normes" }, "Kairouan"),
        new("Yassine Kefi", "climatisation", "Tunis", 9, "Installation et entretien de climatiseurs.", new[] { "Installation", "Entretien", "Recharge gaz" }, "Grand Tunis"),
        new("Bilel Sassi", "serrurier", "Sousse", 8, "Ouverture de porte et serrures sécurisées.", new[] { "Ouverture", "Blindage", "Clés" }, "Sousse"),
        new("Lotfi Rekik", "menuisier", "Sfax", 14, "Portes, placards et cuisines sur mesure.", new[] { "Placards", "Portes", "Cuisines" }, "Sfax")
    };

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);

        var existing = await db.ExecuteScalarAsync<long>(new CommandDefinition("SELECT COUNT(*) FROM users", cancellationToken: ct));
        if (existing > 0) return;

        var trades = (await db.QueryAsync<IdName>(new CommandDefinition(
            "SELECT id AS Id, slug AS Name FROM trades", cancellationToken: ct))).ToDictionary(x => x.Name, x => x.Id);
        var governorates = (await db.QueryAsync<IdName>(new CommandDefinition(
            "SELECT id AS Id, name_fr AS Name FROM governorates", cancellationToken: ct))).ToDictionary(x => x.Name, x => x.Id);

        if (trades.Count == 0 || governorates.Count == 0)
        {
            logger.LogWarning("Reference data is missing: run database/02-reference-data.sql before seeding.");
            return;
        }

        var demoHash = BCrypt.Net.BCrypt.HashPassword("Demo1234!");
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin1234!");

        await using var tx = await db.BeginTransactionAsync(ct);

        async Task<int> InsertUser(string name, string? email, string phone, string hash, string role, int? governorateId) =>
            (int)await db.ExecuteScalarAsync<long>(new CommandDefinition(
                "INSERT INTO users (full_name, email, phone, password_hash, role, governorate_id) " +
                "VALUES (@name, @email, @phone, @hash, @role, @governorateId); SELECT LAST_INSERT_ID();",
                new { name, email, phone, hash, role, governorateId }, tx, cancellationToken: ct));

        await InsertUser("Administrateur", "admin@handyconnect.tn", "+21671000000", adminHash, "admin", governorates["Tunis"]);

        var clientIds = new List<int>();
        for (var i = 0; i < ClientNames.Length; i++)
        {
            clientIds.Add(await InsertUser(ClientNames[i], $"client{i + 1}@example.tn", $"+216210000{i + 1:D2}",
                demoHash, "client", governorates["Tunis"]));
        }

        for (var i = 0; i < Artisans.Length; i++)
        {
            var a = Artisans[i];
            var id = await InsertUser(a.Name, $"artisan{i + 1}@example.tn", $"+216200000{i + 1:D2}",
                demoHash, "artisan", governorates[a.Governorate]);

            await db.ExecuteAsync(new CommandDefinition(
                "INSERT INTO artisan_profiles (user_id, trade_id, bio, zone, experience_years, subscription_ends_at) " +
                "VALUES (@id, @tradeId, @bio, @zone, @years, @ends)",
                new { id, tradeId = trades[a.Trade], bio = a.Bio, zone = a.Zone, years = a.Years, ends = DateTime.UtcNow.AddYears(1) },
                tx, cancellationToken: ct));

            await db.ExecuteAsync(new CommandDefinition(
                "INSERT INTO artisan_specialties (artisan_id, label) VALUES (@id, @label)",
                a.Specialties.Select(label => new { id, label }), tx, cancellationToken: ct));

            // Three approved reviews per artisan, each from a client who "contacted" them.
            for (var k = 0; k < 3; k++)
            {
                var clientIndex = (i + k) % clientIds.Count;
                await db.ExecuteAsync(new CommandDefinition(
                    "INSERT INTO contact_logs (client_id, artisan_id) VALUES (@clientId, @id)",
                    new { clientId = clientIds[clientIndex], id }, tx, cancellationToken: ct));
                await db.ExecuteAsync(new CommandDefinition(
                    "INSERT INTO reviews (artisan_id, client_id, rating, comment, status) VALUES (@id, @clientId, @rating, @comment, 'approved')",
                    new
                    {
                        id,
                        clientId = clientIds[clientIndex],
                        rating = (i + k) % 4 == 0 ? 4 : 5,
                        comment = Comments[(i + k) % Comments.Length]
                    }, tx, cancellationToken: ct));
            }
        }

        await tx.CommitAsync(ct);
        logger.LogInformation("Demo data seeded (password: Demo1234!, admin: admin@handyconnect.tn / Admin1234!).");
    }
}
