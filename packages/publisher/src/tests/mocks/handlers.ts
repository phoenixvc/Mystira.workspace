import { http, HttpResponse } from 'msw';
import { mockStories, mockUsers, mockAuditLogs } from './data';

const API_BASE = 'http://localhost:8080/api';

export const handlers = [
  // Auth handlers
  http.post(`${API_BASE}/auth/login`, async ({ request }) => {
    const body = await request.json() as { email: string; password: string };
    const user = mockUsers.find(u => u.email === body.email);

    if (!user) {
      return HttpResponse.json(
        { success: false, message: 'Invalid credentials' },
        { status: 401 }
      );
    }

    return HttpResponse.json({
      success: true,
      data: {
        user,
        accessToken: 'mock-access-token',
        refreshToken: 'mock-refresh-token',
        expiresAt: new Date(Date.now() + 3600000).toISOString(),
      },
    });
  }),

  http.get(`${API_BASE}/auth/me`, () => {
    return HttpResponse.json({
      success: true,
      data: mockUsers[0],
    });
  }),

  http.post(`${API_BASE}/auth/logout`, () => {
    return HttpResponse.json({ success: true, data: null });
  }),

  // Stories handlers
  http.get(`${API_BASE}/stories`, ({ request }) => {
    const url = new URL(request.url);
    const status = url.searchParams.get('status');
    const search = url.searchParams.get('search');

    let stories = [...mockStories];

    if (status) {
      stories = stories.filter(s => s.status === status);
    }

    if (search) {
      stories = stories.filter(s =>
        s.title.toLowerCase().includes(search.toLowerCase())
      );
    }

    return HttpResponse.json({
      success: true,
      data: {
        items: stories,
        total: stories.length,
        page: 1,
        pageSize: 20,
        hasMore: false,
      },
    });
  }),

  http.get(`${API_BASE}/stories/:id`, ({ params }) => {
    const story = mockStories.find(s => s.id === params.id);

    if (!story) {
      return HttpResponse.json(
        { success: false, message: 'Story not found' },
        { status: 404 }
      );
    }

    return HttpResponse.json({ success: true, data: story });
  }),

  http.post(`${API_BASE}/stories`, async ({ request }) => {
    const body = await request.json() as { title: string; summary: string };
    const newStory = {
      id: `story-${Date.now()}`,
      title: body.title,
      summary: body.summary,
      contributors: [],
      status: 'draft' as const,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    return HttpResponse.json({ success: true, data: newStory }, { status: 201 });
  }),

  // Contributors handlers
  http.get(`${API_BASE}/contributors/story/:storyId`, ({ params }) => {
    const story = mockStories.find(s => s.id === params.storyId);

    if (!story) {
      return HttpResponse.json({ success: true, data: [] });
    }

    return HttpResponse.json({
      success: true,
      data: story.contributors.map((c, i) => ({
        id: `attr-${i}`,
        storyId: story.id,
        userId: c.userId,
        role: c.role,
        split: c.split,
        approvalStatus: c.approvalStatus,
        createdAt: story.createdAt,
        updatedAt: story.updatedAt,
      })),
    });
  }),

  // Audit handlers
  http.get(`${API_BASE}/audit`, () => {
    return HttpResponse.json({
      success: true,
      data: {
        items: mockAuditLogs,
        total: mockAuditLogs.length,
        page: 1,
        pageSize: 20,
        hasMore: false,
      },
    });
  }),

  // User search
  http.get(`${API_BASE}/users/search`, ({ request }) => {
    const url = new URL(request.url);
    const query = url.searchParams.get('query') || '';

    const users = mockUsers.filter(u =>
      u.name.toLowerCase().includes(query.toLowerCase()) ||
      u.email.toLowerCase().includes(query.toLowerCase())
    );

    return HttpResponse.json({ success: true, data: users });
  }),
];
