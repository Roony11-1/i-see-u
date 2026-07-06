const API_BASE = '/api';

async function loadMovies(genre = '') {
    const url = genre ? `${API_BASE}/movies?genre=${genre}` : `${API_BASE}/movies`;
    const res = await fetch(url);
    const movies = await res.json();
    const tbody = document.getElementById('movies-tbody');
    tbody.innerHTML = movies.map(m => `
        <tr>
            <td onclick="showMovieDetail(${m.id})" style="cursor:pointer">${m.title}</td>
            <td onclick="showMovieDetail(${m.id})" style="cursor:pointer">${m.genre}</td>
            <td onclick="showMovieDetail(${m.id})" style="cursor:pointer">${m.releaseYear}</td>
            <td onclick="showMovieDetail(${m.id})" style="cursor:pointer">${m.director}</td>
            <td onclick="showMovieDetail(${m.id})" style="cursor:pointer">${m.reviewCount}</td>
            <td><button class="btn-small" onclick="event.stopPropagation(); openReviewModal(${m.id}, '${m.title.replace(/'/g, "\\'")}')">Reseñar</button></td>
        </tr>
    `).join('');
}

function openReviewModal(movieId, movieTitle) {
    document.getElementById('modal-movie-id').value = movieId;
    document.getElementById('modal-movie-title').textContent = movieTitle;
    document.getElementById('modal-author').value = 'Anonimo';
    document.getElementById('modal-rating').value = 5;
    document.getElementById('modal-comment').value = '';
    document.getElementById('review-modal').style.display = 'flex';
}

async function submitQuickReview() {
    const movieId = parseInt(document.getElementById('modal-movie-id').value);
    const author = document.getElementById('modal-author').value || 'Anonimo';
    const rating = parseInt(document.getElementById('modal-rating').value);
    const comment = document.getElementById('modal-comment').value;
    const res = await fetch(`${API_BASE}/movies/${movieId}/reviews`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ author, rating, comment })
    });
    if (res.ok) {
        document.getElementById('review-modal').style.display = 'none';
        loadMovies(document.getElementById('genre-filter').value);
    } else {
        alert('Error al enviar la reseña');
    }
}

async function showMovieDetail(id) {
    const res = await fetch(`${API_BASE}/movies/${id}`);
    const movie = await res.json();
    const detail = document.getElementById('movie-detail');
    detail.style.display = 'block';
    detail.innerHTML = `
        <h3>${movie.title} (${movie.releaseYear})</h3>
        <p><strong>Director:</strong> ${movie.director}</p>
        <p><strong>Genero:</strong> ${movie.genre}</p>
        <p>${movie.description}</p>
        <hr>
        <h4>Resenas (${movie.reviews.length})</h4>
        <div id="reviews-list">
            ${movie.reviews.map(r => `
                <div class="review">
                    <strong>${'★'.repeat(r.rating)}${'☆'.repeat(5-r.rating)}</strong>
                    <p>${r.comment || ''}</p>
                    <small>por ${r.author} - ${new Date(r.createdAt).toLocaleDateString()}</small>
                </div>
            `).join('')}
        </div>
        <div id="review-form-wrapper">
            <h4>Agregar resena</h4>
            <input id="review-author" placeholder="Tu nombre" value="Anonimo">
            <select id="review-rating">
                ${[1,2,3,4,5].map(n => `<option value="${n}">${'★'.repeat(n)}${'☆'.repeat(5-n)}</option>`).join('')}
            </select>
            <textarea id="review-comment" placeholder="Comentario (opcional)"></textarea>
            <button onclick="submitReview(${id})">Enviar</button>
        </div>
        <button onclick="addToWatchlist(${id})">Agregar a mi watchlist</button>
    `;
    document.getElementById('movie-detail').scrollIntoView({ behavior: 'smooth' });
}

async function submitReview(movieId) {
    const author = document.getElementById('review-author').value || 'Anonimo';
    const rating = parseInt(document.getElementById('review-rating').value);
    const comment = document.getElementById('review-comment').value;
    const res = await fetch(`${API_BASE}/movies/${movieId}/reviews`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ author, rating, comment })
    });
    if (res.ok) {
        showMovieDetail(movieId);
        document.getElementById('review-comment').value = '';
    }
}

async function addToWatchlist(movieId) {
    const user = prompt('Tu nombre:', 'Anonimo') || 'Anonimo';
    const res = await fetch(`${API_BASE}/watchlist`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ movieId, user, status: 'want_to_watch' })
    });
    if (res.ok) loadWatchlist();
}

async function loadWatchlist() {
    const res = await fetch(`${API_BASE}/watchlist`);
    const items = await res.json();
    const div = document.getElementById('watchlist-items');
    div.innerHTML = items.map(w => `
        <div class="watchlist-item">
            <strong>${w.movieTitle}</strong>
            <span class="badge ${w.status}">${w.status === 'want_to_watch' ? 'Pendiente' : 'Vista'}</span>
            <small>por ${w.user}</small>
            <button onclick="toggleWatchlistStatus(${w.id}, '${w.status}')">
                ${w.status === 'want_to_watch' ? 'Marcar vista' : 'Volver a pendiente'}
            </button>
            <button onclick="removeFromWatchlist(${w.id})">Eliminar</button>
        </div>
    `).join('');
}

async function toggleWatchlistStatus(id, currentStatus) {
    const newStatus = currentStatus === 'want_to_watch' ? 'watched' : 'want_to_watch';
    const res = await fetch(`${API_BASE}/watchlist/${id}?status=${newStatus}`, { method: 'PUT' });
    if (res.ok) loadWatchlist();
}

async function removeFromWatchlist(id) {
    const res = await fetch(`${API_BASE}/watchlist/${id}`, { method: 'DELETE' });
    if (res.ok) loadWatchlist();
}

document.addEventListener('DOMContentLoaded', async () => {
    await loadMovies();
    await loadWatchlist();

    document.getElementById('genre-filter').addEventListener('change', (e) => {
        loadMovies(e.target.value);
    });
});
