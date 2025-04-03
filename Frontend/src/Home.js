import {useState, useEffect, useContext} from 'react';
import { Link } from 'react-router-dom';
import { AppContext } from './App';

function Home() {
    const {request} = useContext(AppContext);

    const [categories, setCategories] = useState([]);
    useEffect(() => {
      request('/api/category')
        .then(data => setCategories(data.categories))
        .catch(console.error);
      // fetch("https://localhost:7117/api/category").then(r => r.json()).then(j => {
      //   setCategories(j.data.categories);
      // });
    }, []);
    return (<>
    <div className='category-container'>
        {categories.map(c => 
        <Link to={'/category/' + c.slug} className="card mx-3 h-100 category" key={c.id} title="@Model.Description">
            <img src={c.imagesCsv.split(',')[0]} className='card-img-top' alt='Image'/>
            <div className="card-body">
                <h5 className="card-title">{c.name}</h5>
                <p className="card-text">{c.description}</p>
            </div>
        </Link>
       )}
    </div>
    </>);
  }

  export default Home;