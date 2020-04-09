import React from 'react';
import logo from './logo.svg';
import './App.css';
import Sidebar from './Routing/Sidebar';
import Quote from './Quotes/Quote';
import {
  BrowserRouter as Router,
  Switch,
  Route,
  Link
} from 'react-router-dom'

function App() {
  return (
    <Router>
      <Sidebar/>
      <div className="App">
      <Switch>
        <Route path="/quotes">
          <Quote quoteId={2} content="aa" submitter={false} admin={false}/>
        </Route>
        <Route path="/">
          <h1>Home</h1>
        </Route>
      </Switch>
      </div>
    </Router>
  );
}

export default App;
