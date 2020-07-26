using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tides.Actors;

namespace PinkUmbrella.Services
{
    public interface IExternalDbContext: IDisposable
    {
        DbContext Context { get; set; }

        Peer Peer { get; set; }

        Task SwitchTo(string peerHandle);
    }
}