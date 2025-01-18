using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Infrastructure.Persistence.Configurations;

public class PromptConfiguration
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        builder
            .HasOne(b => b.CreatorUser)
            .WithMany()
            .HasForeignKey(b => b.CreatorUserId);

        builder
            .HasOne(b => b.LastModifierUser)
            .WithMany()
            .HasForeignKey(b => b.LastModifierUserId);
    }
}