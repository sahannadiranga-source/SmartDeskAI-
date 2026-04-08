import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ChatResponse {
  sessionId: string;
  answer: string;
  sentiment: number;
  priority_Escalation: boolean;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private apiUrl = 'http://localhost:5018/api/chat';

  constructor(private http: HttpClient) {}

  sendMessage(message: string, sessionId: string): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(this.apiUrl, { message, sessionId });
  }

  resetSession(sessionId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/session/${sessionId}`);
  }
}
