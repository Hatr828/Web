import { useContext, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import './CategoryProducts.css';
import { AppContext } from "./App";

function Category(){
  const {request} = useContext(AppContext);

    const {id} = useParams();
    const [category, setCategory] = useState({
      "id": "",
      "name": "",
      "description": "",
      "imagesCsv": "",
      "slug": "",
      "products": []}
    );

    useEffect(() => {
      request('/api/category/' + id)
      .then(data => setCategory(data.category))
      .catch(console.error);
        }, [id]
    );

    return <>
      {!!category || <div className="text-center">
        <h1 className="display-4">Крамниця - розділ не знайдено</h1>
        </div>}
      {!!category && <>
        <div className="text-center">
          <h1 className="display-4">{category.name}</h1>
        </div>
        <div className="row row-cols-1 row-cols-md-3 g-4">
          {category.products.map(p => <ProductCard key={p.id} product={p}/>)}
        </div>
      </>}
      
    </>;
  }

  function ProductCard({product}){
    return <>
      <div className="col">

      <div className="card h-100" title="@(Model.Description)">
        <Link to={'/product/' + (product.slug || product.id)}>
          <img src={product.imagesCsv.split(',')[0]} className="card-img-top product-img" alt=""/>
        </Link>
        <div className="card-body">
          <div data-cart-product="@Model.Id" className="card-fab"><i className="bi bi-bag-plus"></i></div>
          <h5 className="card-title">{product.name}</h5>
          <p className="card-text">{product.description}</p>
        </div>
        <div className="card-footer">
          <strong>₴ {product.price}</strong>, у наявності - {product.stock}
        </div>
      </div>
      </div>
    </>
  }

export default Category;