using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Peer;

namespace PinkUmbrella.Services
{
    public interface IExternalDbContext: IDisposable
    {
        DbContext Context { get; set; }

        PeerModel Peer { get; set; }

        Task SwitchTo(string peerHandle);
    }
}