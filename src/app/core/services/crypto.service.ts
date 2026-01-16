import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CryptoService {
  
   private secretKey = 'mySecretKey123!';

  encrypt(data: string): string {
    return btoa(`${this.secretKey}:${data}`);
  }

  decrypt(encrypted: string): string {
    const decoded = atob(encrypted);
    return decoded.replace(`${this.secretKey}:`, '');
  }
}
