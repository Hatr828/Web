import './App.css';
import { BrowserRouter, Routes, Route} from 'react-router-dom';
import Home from './Home.js';
import Category from './Category.js';
import Layout from './Layout.js';
import Product from './views/product/product.jsx';
import { createContext } from 'react';

export const AppContext = createContext(null); 

function App(){
  const request = (url, conf) => new Promise((resolve, reject) =>  {
    if(url.startsWith('/')){
      url = "https://localhost:7117" + url;
    }
    fetch(url, conf)
    .then(r => r.json())
      .then(j => {
        if(j.status.isSuccess){
          resolve(j.data);
        }
        else {
          reject(j);
        }
      })
  });

  return <AppContext.Provider value={{request}}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Layout />} >
            <Route index element={<Home />} />
            <Route path="category/:id" element={<Category />} />
            <Route path="product/:id" element={<Product />} />
          </Route>
        </Routes>
      </BrowserRouter>
  </AppContext.Provider> 
  
}

export default App;
