import React, { useState, useEffect } from 'react';
import './App.css';

function App() {
  const [activeTab, setActiveTab] = useState('library');
  const [books, setBooks] = useState([]);
  const [users, setUsers] = useState([]);
  const [history, setHistory] = useState([]);
  const [activeCard, setActiveCard] = useState(''); 
  const [searchQuery, setSearchQuery] = useState('');
  
  // UI States
  const [showAddModal, setShowAddModal] = useState(false);
  const [toast, setToast] = useState({ show: false, message: '', type: '' });

  // Form States
  const [newTitle, setNewTitle] = useState('');
  const [newAuthor, setNewAuthor] = useState('');
  const [newStock, setNewStock] = useState(1);
  const [newUserName, setNewUserName] = useState('');

  const API_URL = 'https://localhost:7046/api/books'; 

  // --- CUSTOM TOAST NOTIFICATIONS ---
  const showToast = (message, type = 'success') => {
    setToast({ show: true, message, type });
    setTimeout(() => setToast({ show: false, message: '', type: '' }), 3000);
  };

  // --- API CALLS ---
  const fetchBooks = () => {
    const url = searchQuery ? `${API_URL}/author?name=${encodeURIComponent(searchQuery)}` : API_URL;
    fetch(url).then(res => res.json()).then(setBooks).catch(console.error);
  };
  const fetchUsers = () => fetch(`${API_URL}/users`).then(res => res.json()).then(setUsers).catch(console.error);
  const fetchHistory = () => fetch(`${API_URL}/history`).then(res => res.json()).then(setHistory).catch(console.error);

  useEffect(() => { fetchBooks(); fetchUsers(); fetchHistory(); }, [searchQuery]);

  // --- CORE ACTIONS ---
  const handleBorrow = (title) => {
    if (!activeCard) return showToast("Please enter your Library Card above!", "error");
    fetch(`${API_URL}/${activeCard}/borrow?title=${encodeURIComponent(title)}`, { method: 'PUT' })
      .then(async res => { if (!res.ok) throw new Error(await res.text()); showToast(`Borrowed ${title}!`); fetchBooks(); fetchHistory(); })
      .catch(err => showToast(err.message, "error"));
  };

  const handleReturn = (title) => {
    if (!activeCard) return showToast("Please enter your Library Card above!", "error");
    fetch(`${API_URL}/Return/${activeCard}?title=${encodeURIComponent(title)}`, { method: 'PUT' })
      .then(async res => { if (!res.ok) throw new Error(await res.text()); showToast(`Returned ${title}!`); fetchBooks(); fetchHistory(); })
      .catch(err => showToast(err.message, "error"));
  };

  const handleDelete = (title) => {
    if (!window.confirm(`Permanently delete ${title}?`)) return;
    fetch(`${API_URL}/${encodeURIComponent(title)}`, { method: 'DELETE' })
      .then(() => { showToast("Book deleted."); fetchBooks(); })
      .catch(console.error);
  };

  const handleAddBook = (e) => {
    e.preventDefault();
    fetch(API_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ bookTitle: newTitle, author: newAuthor, stock: parseInt(newStock) })
    }).then(async res => {
      if (!res.ok) throw new Error(await res.text());
      showToast("Book successfully added to library!");
      fetchBooks(); setShowAddModal(false); setNewTitle(''); setNewAuthor(''); setNewStock(1);
    }).catch(err => showToast(err.message, "error"));
  };

  const handleAddUser = (e) => {
    e.preventDefault();
    fetch(`${API_URL}/user`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ fullName: newUserName })
    }).then(res => res.json()).then(data => { 
      showToast(`User registered! Card: ${data.cardNumber}`); 
      fetchUsers(); setNewUserName(''); 
    }).catch(err => showToast(err.message, "error"));
  };

  return (
    <div className="dashboard">
      {/* Toast Notification Container */}
      {toast.show && <div className={`toast ${toast.type}`}>{toast.message}</div>}

      <div className="header">
        <h1>Nexus Library System</h1>
        <div className="nav-tabs glass-panel">
          <button className={`nav-btn ${activeTab === 'library' ? 'active' : ''}`} onClick={() => setActiveTab('library')}>📚 Collection</button>
          <button className={`nav-btn ${activeTab === 'users' ? 'active' : ''}`} onClick={() => setActiveTab('users')}>👥 Members</button>
          <button className={`nav-btn ${activeTab === 'history' ? 'active' : ''}`} onClick={() => setActiveTab('history')}>⏱️ Activity Log</button>
        </div>
      </div>

      {/* --- TAB 1: LIBRARY --- */}
      {activeTab === 'library' && (
        <>
          <div className="control-bar glass-panel">
            <input type="text" className="input-field search-box" placeholder="🔍 Search by Author..." value={searchQuery} onChange={e => setSearchQuery(e.target.value)} />
            
            <div style={{display: 'flex', gap: '15px', alignItems: 'center'}}>
              <span style={{color: 'var(--text-muted)'}}>Active Card:</span>
              <input type="text" className="input-field" placeholder="ID Number" value={activeCard} onChange={e => setActiveCard(e.target.value)} style={{width: '120px'}} />
              <button className="btn btn-primary" onClick={() => setShowAddModal(true)}>➕ New Book</button>
            </div>
          </div>

          <div className="book-grid">
            {books.map(book => (
              <div key={book.bookTitle} className="book-card glass-panel">
                <h3 className="book-title">{book.bookTitle}</h3>
                <p className="book-author">{book.author}</p>
                <div className="book-stats">
                  <span>📦 Stock: {book.stock}/{book.maxStock}</span>
                  <span>📖 Readers: {book.currentBorrowerLibraryCard?.length || 0}</span>
                </div>
                <div className="card-actions">
                  <button className="btn btn-success" style={{flex: 1}} onClick={() => handleBorrow(book.bookTitle)}>Borrow</button>
                  <button className="btn btn-success" style={{flex: 1}} onClick={() => handleReturn(book.bookTitle)}>Return</button>
                  <button className="btn btn-danger" onClick={() => handleDelete(book.bookTitle)}>🗑️</button>
                </div>
              </div>
            ))}
          </div>

          {/* Add Book Modal */}
          {showAddModal && (
            <div className="modal-overlay" onClick={() => setShowAddModal(false)}>
              <div className="modal-content glass-panel" onClick={e => e.stopPropagation()}>
                <h2>Add to Collection</h2>
                <form onSubmit={handleAddBook} className="form-column">
                  <input type="text" className="input-field" placeholder="Book Title" value={newTitle} onChange={e => setNewTitle(e.target.value)} required minLength="3" />
                  <input type="text" className="input-field" placeholder="Author Name" value={newAuthor} onChange={e => setNewAuthor(e.target.value)} required minLength="3" />
                  <input type="number" className="input-field" placeholder="Initial Stock" value={newStock} onChange={e => setNewStock(e.target.value)} required min="1" />
                  <div style={{display: 'flex', gap: '10px', marginTop: '10px'}}>
                    <button type="button" className="btn btn-danger" style={{flex: 1}} onClick={() => setShowAddModal(false)}>Cancel</button>
                    <button type="submit" className="btn btn-primary" style={{flex: 2}}>Save Book</button>
                  </div>
                </form>
              </div>
            </div>
          )}
        </>
      )}

      {/* --- TAB 2: USERS --- */}
      {activeTab === 'users' && (
        <div className="glass-panel" style={{padding: '30px'}}>
          <h2 style={{marginTop: 0}}>Register Member</h2>
          <form onSubmit={handleAddUser} style={{display: 'flex', gap: '15px', marginBottom: '40px'}}>
            <input type="text" className="input-field" style={{flex: 1}} placeholder="Full Legal Name" value={newUserName} onChange={e => setNewUserName(e.target.value)} required />
            <button type="submit" className="btn btn-primary">Generate Card ID</button>
          </form>

          <h3 style={{color: 'var(--text-muted)'}}>Active Members</h3>
          <table className="data-table">
            <tbody>
              {users.map(u => (
                <tr key={u.libraryCard}>
                  <td style={{fontWeight: '600'}}>{u.fullName}</td>
                  <td style={{textAlign: 'right'}}><code style={{background: 'rgba(0,0,0,0.3)', padding: '5px 10px', borderRadius: '4px', color: 'var(--accent)'}}>{u.libraryCard}</code></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* --- TAB 3: HISTORY --- */}
      {activeTab === 'history' && (
        <div className="glass-panel" style={{padding: '30px'}}>
          <h2 style={{marginTop: 0}}>System Logs</h2>
          <table className="data-table">
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Action</th>
                <th>Resource</th>
                <th>User ID</th>
              </tr>
            </thead>
            <tbody>
              {history.slice().reverse().map((h, i) => (
                <tr key={i}>
                  <td>{new Date(h.timestamp).toLocaleString()}</td>
                  <td><span style={{color: h.action === 'Borrow' ? 'var(--danger)' : 'var(--success)'}}>{h.action}</span></td>
                  <td style={{fontWeight: '500'}}>{h.bookTitle}</td>
                  <td><code style={{color: 'var(--text-muted)'}}>{h.borrowerLibCard}</code></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default App;