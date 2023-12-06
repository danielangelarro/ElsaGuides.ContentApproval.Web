using Microsoft.EntityFrameworkCore;

namespace ElsaGuides.ContentApproval.Web.Entity
{
    public class Register
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public bool IsConfirmed1 { get; set; }
        public bool IsConfirmed2 { get; set; }
        public bool IsActivate { get; set; }
    }
}