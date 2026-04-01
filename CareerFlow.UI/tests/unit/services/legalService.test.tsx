import {getLegal} from '@/services/legalService';
import {api, API_URL} from '@/services/utils';

jest.mock('@/services/utils', () => ({
  API_URL: 'https://mock-api.example.com/api',
  api: {
    get: jest.fn(),
  },
}));

describe('Legal API Service', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('getLegal', () => {
    it('should call api.get with the correct URL and document type query parameter', async () => {
      const mockDocumentType = 'terms-of-service';
      const mockResponse = {
        data: {id: 1, title: 'Terms of Service', content: 'Content'},
      };

      (api.get as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await getLegal(mockDocumentType);

      expect(api.get).toHaveBeenCalledTimes(1);
      expect(api.get).toHaveBeenCalledWith(
        `${API_URL}/legal?type=${mockDocumentType}`,
      );
      expect(result).toEqual(mockResponse);
    });
  });
});
