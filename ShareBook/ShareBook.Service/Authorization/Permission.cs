using System.Collections.Generic;

namespace ShareBook.Service.Authorization
{
    public class Permissions
    {
        public enum Permission
        {
            CriarLivro,
            EditarLivro,
            ExcluirLivro,
            AprovarLivro,
        }

        public static List<Permission> AdminPermissions { get; } = new List<Permission>() { Permission.AprovarLivro };
    }
}
