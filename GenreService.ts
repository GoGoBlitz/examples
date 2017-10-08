import {Http, Headers} from 'angular2/http';
import {Injectable} from 'angular2/core';
import 'rxjs/operators/map';

@Injectable()
export class GenreService {
    private genres: any[] = [];

    constructor(private http: Http) {

    }

    ///Заполние возможных жанров музыки
    public initialize(callback): any {
        this.http.get('/api/genres').map(res => res.json()).subscribe(data => {
            this.genres = data;
            callback();
        });
    }

    public getGenres(): any[] {
        return this.genres;
    }

    //Получить случайную выборку движений из всех возможных
    public getRandomMovementsFromGenre(genre): any[] {
        let result: any[] = [];

        let randomMovementsCountToAdd = Math.floor(Math.random() * genre.movements.length);

        while (randomMovementsCountToAdd > result.length) {
            var randomMove = genre.movements[Math.floor(Math.random() * genre.movements.length)];

            while (result.find(d => (d === randomMove)) != undefined) {
                randomMove = genre.movements[Math.floor(Math.random() * genre.movements.length)];
            }

            result.push(randomMove);
        }

        return result;
    }
}