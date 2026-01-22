const API_BASE = "/api/fitness";

async function saveActivity() {
    const user = document.getElementById('username').value;
    const dist = document.getElementById('distance').value;
    const coords = document.getElementById('location-select').value.split(',');

    if (!user || !dist) {
        alert("Popunite sva polja!");
        return;
    }

    const payload = {
        username: user,
        distance: parseFloat(dist),
        latitude: parseFloat(coords[0]),
        longitude: parseFloat(coords[1])
    };

    try {
        // 1. Snimanje podataka (Atomičnost u Redisu)
        await axios.post(`${API_BASE}/activity`, payload);

        // 2. Radar pretraga (GEO)
        const res = await axios.get(`${API_BASE}/radar?lat=${payload.latitude}&lon=${payload.longitude}`);
        updateRadarList(res.data);

        // 3. Osvežavanje Dashboard-a
        refreshDashboard();
        
    } catch (err) {
        console.error("Greška pri radu sa Redisom:", err);
    }
}

function updateRadarList(runners) {
    const list = document.getElementById('nearby-list');
    const count = document.getElementById('nearby-count');
    list.innerHTML = "";
    count.innerText = `${runners.length} trkača`;

    if (runners.length === 0) {
        list.innerHTML = `
            <div class="text-center py-20 opacity-30">
                <p class="text-xs">Nema aktivnih trkača u radijusu od 5km.</p>
            </div>`;
        return;
    }

    runners.forEach(r => {
        list.innerHTML += `
            <div class="nearby-item flex justify-between items-center">
                <div>
                    <div class="text-white font-bold text-sm">${r.username}</div>
                    <div class="text-[10px] text-blue-400 uppercase tracking-tighter">Lokacija aktivna</div>
                </div>
                <div class="text-right">
                    <div class="text-blue-300 font-mono text-xs">${r.distance.toFixed(2)} km</div>
                    <div class="text-[9px] text-zinc-600 uppercase">udaljenost</div>
                </div>
            </div>`;
    });
}

async function refreshDashboard() {
    try {
        const res = await axios.get(`${API_BASE}/dashboard`);
        const { leaderboard, record, feed } = res.data;

        // Leaderboard
        document.getElementById('leaderboard').innerHTML = leaderboard.map((p, i) => `
            <div class="flex justify-between items-center p-3 bg-black/40 rounded-xl border border-zinc-800/50">
                <span class="text-zinc-500 font-bold text-xs">#${i+1}</span>
                <span class="text-sm font-semibold">${p.username}</span>
                <span class="text-[#00ff99] font-mono text-xs">${p.distance.toFixed(1)} km</span>
            </div>
        `).join('');

        // Globalni Rekord
        document.getElementById('global-record').innerText = `${record.toFixed(1)} km`;

        // Activity Feed
        document.getElementById('feed').innerHTML = feed.map(msg => `
            <div class="py-1 border-b border-zinc-800/30">${msg}</div>
        `).join('');

    } catch (e) {
        console.log("Server nije dostupan...");
    }
}

// Event Listeners
document.getElementById('btnSave').addEventListener('click', saveActivity);

// Inicijalno učitavanje i auto-refresh (10s)
refreshDashboard();
setInterval(refreshDashboard, 10000);