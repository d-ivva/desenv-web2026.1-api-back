// Data/AppDbContext.cs

// Importações necessárias:
// - Microsoft.EntityFrameworkCore: namespace do EF Core (DbContext, DbSet, etc.)
// - MeuCrud.Api.Models: namespace onde estão nossas classes de modelo
using DesenvWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DesenvWebApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet existente — tabela "Produtos"
    public DbSet<Produto> Produtos { get; set; }

    // =====================================================================
    // NOVO: DbSet para Categorias
    //
    // Ao adicionar este DbSet, estamos dizendo ao EF:
    // "Existe uma tabela chamada 'Categorias' no banco de dados,
    //  e cada linha dessa tabela corresponde a um objeto Categoria."
    //
    // A partir de agora, podemos usar:
    //   _context.Categorias.ToListAsync()     → SELECT * FROM "Categorias"
    //   _context.Categorias.FindAsync(id)     → SELECT * WHERE Id = @id
    //   _context.Categorias.Add(categoria)    → prepara INSERT
    // =====================================================================
    public DbSet<Categoria> Categorias { get; set; }

    // =====================================================================
    // OnModelCreating — configuração avançada do modelo
    //
    // Este método é chamado pelo EF quando ele está "montando" o modelo
    // do banco de dados. Aqui usamos a Fluent API para configurar
    // relacionamentos, restrições e comportamentos.
    //
    // Fluent API vs Data Annotations:
    // - Data Annotations: [Required], [MaxLength(100)], [ForeignKey("...")]
    //   São atributos colocados diretamente nas propriedades do model.
    //
    // - Fluent API: configuração feita aqui no OnModelCreating.
    //   É mais poderosa e permite configurações que Data Annotations não suportam.
    //
    // Neste curso, usamos Fluent API para deixar os relacionamentos
    // explícitos e fáceis de entender em um único lugar.
    // =====================================================================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =================================================================
        // CONFIGURAÇÃO DO RELACIONAMENTO 1-PARA-N: Produto → Categoria
        //
        // Leia da seguinte forma, de cima para baixo:
        //   "Um Produto TEM UMA Categoria"
        //   "Uma Categoria TEM MUITOS Produtos"
        //   "A chave estrangeira é CategoriaId"
        //   "Se a Categoria for deletada, RESTRINJA (não permita)"
        // =================================================================
        modelBuilder.Entity<Produto>()
            // Um Produto tem uma Categoria (propriedade de navegação)
            .HasOne(p => p.Categoria)
            // Uma Categoria tem muitos Produtos (navegação inversa)
            .WithMany(c => c.Produtos)
            // A chave estrangeira na tabela Produtos é CategoriaId
            .HasForeignKey(p => p.CategoriaId)
            // Comportamento ao deletar: O que acontece com os Produtos
            // quando a Categoria deles é deletada?
            //
            // Restrict  = PROÍBE deletar a categoria se tiver produtos.
            //             O banco lança um erro. É a opção mais segura.
            //
            // Cascade   = Deleta todos os produtos junto com a categoria.
            //             Perigoso! Uma deleção pode remover muitos dados.
            //
            // SetNull   = Define CategoriaId como NULL nos produtos órfãos.
            //             Só funciona se CategoriaId for nullable (int?).
            //
            // Escolhemos Restrict porque é mais seguro para um sistema real:
            // o usuário deve primeiro remover ou reatribuir os produtos
            // antes de deletar uma categoria.
            .OnDelete(DeleteBehavior.Restrict);
    }
}