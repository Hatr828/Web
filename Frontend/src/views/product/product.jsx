import './Product.css';
import { useContext, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { AppContext } from '../../App';

export default function Product(){
    const {request} = useContext(AppContext);

    const {id} = useParams();
    const [product, setProduct] = useState({
      "id": "",
      "name": "",
      "description": "",
      "imagesCsv": "",
      "slug": "",
      "products": []}
    );

    useEffect(() => {
        request('/api/product/' + id)
        .then(data => setProduct(data.product))
        .catch(console.error);
        }, [id]
    );

    return <>
        <h1>Сторінка товару {product.name}</h1>
    </>;
}