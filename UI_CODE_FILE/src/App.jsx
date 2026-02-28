import { useState, useEffect } from 'react';
import './App.css';

function App() {
  const [activeTab, setActiveTab] = useState('library');
  const [books, setBooks] = useState([]);
  const [users, setUsers] = useState([]);
  const [history, setHistory] = useState([]);
  const [activeCard, setActiveCard] = useState(''); 
  
  // Search & Admin States
  const [searchQuery, setSearchQuery] = useState('');
  const [isAdmin, setIsAdmin] = useState(false);
  const [adminPassword, setAdminPassword] = useState('');
  const [amountToAdd, setAmountToAdd] = useState(5);

  // Form States
  const [newTitle, setNewTitle] = useState('');
  const [newAuthor, setNewAuthor] = useState('');
  const [newStock, setNewStock] = useState(1);
  const [newUserName, setNewUserName] = useState('');

  const API_URL = 'https://localhost:7046/api/books'; 

  // --- API CALLS ---
  const fetchBooks = () => {
    // If search is empty, get all. If searching, use the Author filter from your Controller!
    const url = searchQuery 
      ? `${API_URL}/author?name=${encodeURIComponent(searchQuery)}` 
      : API_URL;
      
    fetch(url).then(res => res.json()).then(data => setBooks(data)).catch(console.error);
  };

  const fetchUsers = () => fetch(`${API_URL}/users`).then(res => res.json()).then(setUsers).catch(console.error);
  const fetchHistory = () => fetch(`${API_URL}/history`).then(res => res.json()).then(setHistory).catch(console.error);

  useEffect(() => { fetchBooks(); fetchUsers(); fetchHistory(); }, [searchQuery]);

  // --- CORE ACTIONS (BORROW/RETURN/DELETE) ---
  const handleBorrow = (title) => {
    if (!activeCard) return alert("Enter a Library Card!");
    fetch(`${API_URL}/${activeCard}/borrow?title=${encodeURIComponent(title)}`, { method: 'PUT' })
      .then(async res => { if (!res.ok) throw new Error(await res.text()); fetchBooks(); fetchHistory(); })
      .catch(err => alert(err.message));
  };

  const handleReturn = (title) => {
    if (!activeCard) return alert("Enter a Library Card!");
    fetch(`${API_URL}/Return/${activeCard}?title=${encodeURIComponent(title)}`, { method: 'PUT' })
      .then(async res => { if (!res.ok) throw new Error(await res.text()); fetchBooks(); fetchHistory(); })
      .catch(err => alert(err.message));
  };

  const handleDelete = (title) => {
    if (!window.confirm(`Delete ${title}?`)) return;
    fetch(`${API_URL}/${encodeURIComponent(title)}`, { method: 'DELETE' }).then(() => fetchBooks()).catch(console.error);
  };

  // --- ADMIN INVENTORY EXPANSION (The PUT Admin route) ---
  const handleAdminUpdate = (title) => {
    fetch(`${API_URL}/admin/inventory/${encodeURIComponent(title)}?amountToAdd=${amountToAdd}`, {
      method: 'PUT',
      headers: { 'X-Admin-Password': adminPassword }
    })
    .then(async res => {
      if (!res.ok) throw new Error(await res.text());
      alert("Inventory Expanded!");
      fetchBooks();
    })
    .catch(err => alert(`Admin Error: ${err.message}`));
  };

  const handleAddUser = (e) => {
    e.preventDefault();
    fetch(`${API_URL}/user`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ fullName: newUserName })
    }).then(res => res.json()).then(data => { alert(`Card: ${data.cardNumber}`); fetchUsers(); setNewUserName(''); });
  };

  const handleAddBook = (e) => {
    e.preventDefault();
    fetch(API_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ bookTitle: newTitle, author: newAuthor, stock: parseInt(newStock) })
    }).then(() => { fetchBooks(); setNewTitle(''); setNewAuthor(''); }).catch(console.error);
  };

  return (
    <div className="dashboard">
      <div className="header">
        <h1>📚 Complete Library System</h1>
        <div style={{ display: 'flex', justifyContent: 'center', gap: '10px', marginTop: '15px' }}>
          <button className={`btn ${activeTab === 'library' ? 'btn-primary' : ''}`} onClick={() => setActiveTab('library')}>Library</button>
          <button className={`btn ${activeTab === 'users' ? 'btn-primary' : ''}`} onClick={() => setActiveTab('users')}>Users</button>
          <button className={`btn ${activeTab === 'history' ? 'btn-primary' : ''}`} onClick={() => setActiveTab('history')}>Logs</button>
          <button className={`btn ${isAdmin ? 'btn-danger' : ''}`} onClick={() => setIsAdmin(!isAdmin)}>Admin Mode: {isAdmin ? 'ON' : 'OFF'}</button>
        </div>
      </div>

      {activeTab === 'library' && (
        <>
          {isAdmin && (
            <div className="panel" style={{border: '2px solid #dc3545'}}>
              <h3>🛡️ Admin Control Panel</h3>
              <div className="form-row">
                <input type="password" placeholder="Admin Password" value={adminPassword} onChange={e => setAdminPassword(e.target.value)} />
                <input type="number" placeholder="Amt to add" value={amountToAdd} onChange={e => setAmountToAdd(e.target.value)} style={{maxWidth: '120px'}} />
                <p style={{fontSize: '0.8em'}}>Click "Expand" on any book below to use this password.</p>
              </div>
            </div>
          )}

          <div className="library-card-login">
            <input type="text" placeholder="🔍 Search by Author..." value={searchQuery} onChange={e => setSearchQuery(e.target.value)} style={{marginRight: '20px', width: '200px'}} />
            <strong>🆔 Active Card:</strong>
            <input type="text" value={activeCard} onChange={e => setActiveCard(e.target.value)} style={{width: '150px'}} />
          </div>

          <div className="panel">
            <h3>➕ Add Book</h3>
            <form onSubmit={handleAddBook} className="form-row">
              <input type="text" placeholder="Title" value={newTitle} onChange={e => setNewTitle(e.target.value)} required />
              <input type="text" placeholder="Author" value={newAuthor} onChange={e => setNewAuthor(e.target.value)} required />
              <input type="number" value={newStock} onChange={e => setNewStock(e.target.value)} style={{width: '70px'}} />
              <button type="submit" className="btn btn-primary">Save</button>
            </form>
          </div>

          <div className="book-grid">
            {books.map(book => (
              <div key={book.bookTitle} className="book-card">
                <h3 className="book-title">{book.bookTitle}</h3>
                <p className="book-author">By {book.author}</p>
                <div className="book-stats">Stock: {book.stock} / {book.maxStock}</div>
                <div className="card-actions">
                  <button className="btn btn-action" onClick={() => handleBorrow(book.bookTitle)}>Borrow</button>
                  <button className="btn btn-action" onClick={() => handleReturn(book.bookTitle)}>Return</button>
                  {isAdmin && <button className="btn btn-primary" onClick={() => handleAdminUpdate(book.bookTitle)}>Expand</button>}
                  <button className="btn btn-danger" onClick={() => handleDelete(book.bookTitle)}>Delete</button>
                </div>
              </div>
            ))}
          </div>
        </>
      )}

      {activeTab === 'users' && (
        <div className="panel">
          <h2>Register User</h2>
          <form onSubmit={handleAddUser} className="form-row">
            <input type="text" placeholder="Full Name" value={newUserName} onChange={e => setNewUserName(e.target.value)} required />
            <button type="submit" className="btn btn-action">Register</button>
          </form>
          <hr />
          {users.map(u => <div key={u.libraryCard} style={{padding: '5px'}}><strong>{u.fullName}</strong> - <code>{u.libraryCard}</code></div>)}
        </div>
      )}

      {activeTab === 'history' && (
        <div className="panel">
          <h2>Activity Log</h2>
          {history.map((h, i) => <div key={i} style={{fontSize: '0.9em', borderBottom: '1px solid #eee', padding: '5px'}}>
            [{new Date(h.timestamp).toLocaleTimeString()}] <b>{h.action}</b>: {h.bookTitle} (Card: {h.borrowerLibCard})
          </div>)}
        </div>
      )}
    </div>
  );
}

export default App;